using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Messages;
using IdentityApi.Models;
using MessageService.MessageServices;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly IMessageService _messageService;
        private readonly IMessageProvider _messageProvider;
        private readonly IUserLocationManager _userLocationManager;
        private readonly ILogger<UserManager> _logger;
        public UserManager(IUserProvider userProvider, IMessageService messageService, IMessageProvider messageProvider, ILogger<UserManager> logger, IUserLocationManager userLocationManager)
        {
            _userProvider = userProvider;
            _userLocationManager = userLocationManager;
            _logger = logger;
            _messageService = messageService;
            _messageProvider = messageProvider;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new IpBlockedException();

            // check if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userCreate.Email);
            // User already in use 
            if (existingUser != null)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userCreate.Email}] cannot be created because they already exists");
                throw new UserAlreadyExistsException();
            }


            // TODO: create mapper

            var toCreateDbUser = new DbUser()
            {
                Email = userCreate.Email,
                FirstName = userCreate.FirstName,
                LastName = userCreate.LastName,
                PhoneNumber = userCreate.PhoneNumber
            };

            toCreateDbUser.Salt = Security.GetSalt(50);
            toCreateDbUser.HashedPassword = Security.GetEncryptedAndSaltedPassword(userCreate.Password, toCreateDbUser.Salt);
            toCreateDbUser.Counter = 0;
            toCreateDbUser.SecretKey = Security.GetHmacKey();

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);
            _logger.LogInformation($"User[{userCreate.Email}] has been created");

            userLocation.UserID = createdUser.ID;
            userLocation.Successful = true;
            await _userLocationManager.LogLocationAsync(userLocation);

            var registerMessage = _messageProvider.GetRegisterMessage(createdUser.Email, createdUser.SecretKey);

            _messageService.SendMessageAsync(registerMessage);

            return new User()
            {
                ID = createdUser.ID,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                PhoneNumber = createdUser.PhoneNumber
            };
        }

        public async Task<User> GetUserByIDAsync(int ID)
        {
            var dbUser = await _userProvider.GetUserByIDAsync(ID);

            if (dbUser == null)
                return null;

            // TODO: Add mapper

            return new User()
            {
                ID = dbUser.ID,
                Email = dbUser.Email,
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                PhoneNumber = dbUser.PhoneNumber
            };
        }

        /// <inheritdoc/>
        public async Task<User> LoginAsync(UserLogin userLogin, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new IpBlockedException();

            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
            {
                await _userLocationManager.LogLocationAsync(userLocation);

                return null;
            }

            // Set user id for the rest of the location logs
            userLocation.UserID = existingUser.ID;

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] has been locked");
                throw new AccountLockedException();
            }


            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // if user was not logged in from this location before
                if (!await _userLocationManager.UserWasLoggedInFromLocationAsync(userLocation))
                {
                    if (existingUser.LastRequestDate.HasValue && existingUser.LastRequestDate.Value.AddMinutes(15) < DateTime.Now)
                    {
                        //TODO: Handle multiple logins in an attempt to generate more OTPS
                        //Maybe just return or throw error
                    }
                    existingUser = await _userProvider.UpdateUserLoginNewLocation(existingUser.ID);
                    var hotp = Security.GetHotp(existingUser.SecretKey, existingUser.Counter);
                    if (hotp != null)
                    {
                        var loginFromAnotherLocationEmail = _messageProvider.GetLoginAttemptMessage(existingUser.Email, hotp);

                        _messageService.SendMessageAsync(loginFromAnotherLocationEmail);
                    }
                    await _userLocationManager.LogLocationAsync(userLocation);
                    throw new Required2FAException();
                }
                else
                {
                    // login success 
                    existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);
                    _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");
                }
                return existingUser;
            }
            else
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);


                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }
        }

        /// <inheritdoc/>
        public async Task<User> Login2FaAsync(UserLogin userLogin, UserLocation userLocation)
        {
            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
            {
                return null;
            }

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] has been locked");
                throw new AccountLockedException();
            }

            // TODO: Add in SP
            if (existingUser.LastRequestDate.HasValue && existingUser.LastRequestDate.Value.AddMinutes(15) < DateTime.Now)
            {
                //TODO: Log, maybe a session expired exception
                throw new Exception("Login failed, password expired");
            }

            // check if given otp password matches what is expected
            if (userLogin.Password == Security.GetHotp(existingUser.SecretKey, existingUser.Counter))
            {

                // login success 
                existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);
                userLocation.Successful = true;
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");

                return existingUser;
            }
            else
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);


                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing with 2 step");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }
        }
    }
}

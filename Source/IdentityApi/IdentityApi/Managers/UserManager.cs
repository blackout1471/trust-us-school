using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly IUserLocationManager _userLocationManager;
        private readonly ILogger<UserManager> _logger;
        public UserManager(IUserProvider userProvider, ILogger<UserManager> logger, IUserLocationManager userLocationManager)
        {
            _userProvider = userProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new Exception("login failed");

            // check if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userCreate.Email);

            // User already in use 
            if (existingUser != null)
            {
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

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);
            _logger.LogInformation($"User[{userCreate.Email}] has been created");

            userLocation.UserID = createdUser.ID;
            userLocation.Successful = true;
            await _userLocationManager.LogLocationAsync(userLocation);

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
                FirstName = dbUser.FirstName
            };
        }

        /// <inheritdoc/>
        public async Task<User> LoginAsync(UserLogin userLogin, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new Exception("login failed");

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
                _logger.LogWarning($"User[{userLogin.Email}] has been locked");
                throw new AccountLockedException();
            }


            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // if user was not logged in from this location before
                if (!await _userLocationManager.UserWasLoggedInFromLocationAsync(userLocation))
                {
                    // send 2fa here
                    await _userLocationManager.LogLocationAsync(userLocation);
                    throw new Exception("forbidden");
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
    }
}

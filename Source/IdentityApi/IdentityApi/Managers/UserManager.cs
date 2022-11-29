using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Mapster;
using System.Security.Cryptography;
using System.Text;
using MessageService.MessageServices;
using MessageService.Providers;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly IMessageService _messageService;
        private readonly IMessageProvider _messageProvider;
        private readonly IUserLocationManager _userLocationManager;
        private readonly ILeakedPasswordProvider _leakedPasswordProvider;
        private readonly ILogger<UserManager> _logger;
        public UserManager(IUserProvider userProvider, IMessageService messageService, IMessageProvider messageProvider, ILogger<UserManager> logger, IUserLocationManager userLocationManager, ILeakedPasswordProvider leakedPasswordProvider)
        {
            _userProvider = userProvider;
            _userLocationManager = userLocationManager;
            _leakedPasswordProvider = leakedPasswordProvider;
            _logger = logger;
            _messageService = messageService;
            _messageProvider = messageProvider;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new IpBlockedException();

            // Check if password has been leaked
            if (await CheckPasswordLeakedForUser(userCreate.Password))
            {
                _logger.LogWarning($"User[{userCreate.Email}] tried to register leaked password");
                throw new PasswordLeakedException();
            }

            // User already in use 
            if (await _userProvider.GetUserByEmailAsync(userCreate.Email) != null)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userCreate.Email}] cannot be created because they already exists");
                throw new UserAlreadyExistsException();
            }

            // Map from user create to db user
            var toCreateDbUser = userCreate.Adapt<DbUser>();

            toCreateDbUser.Salt = Security.GetSalt(50);
            toCreateDbUser.HashedPassword = Security.GetEncryptedAndSaltedPassword(userCreate.Password, toCreateDbUser.Salt);
            //TODO: Make a random counter start
            toCreateDbUser.Counter = 0;
            toCreateDbUser.SecretKey = Security.GetHmacKey();

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);
            _logger.LogInformation($"User[{userCreate.Email}] has been created");

            userLocation.UserID = createdUser.ID;
            userLocation.Successful = true;
            await _userLocationManager.LogLocationAsync(userLocation);
            var registerMessage = _messageProvider.GetRegisterMessage(createdUser.Email, createdUser.SecretKey);

            await _messageService.SendMessageAsync(registerMessage);

            // Map from db user to user
            return createdUser.Adapt<User>();
        }

        /// <inheritdoc/>
        public async Task<User> GetUserByIDAsync(int ID)
        {
            var dbUser = await _userProvider.GetUserByIDAsync(ID);
            return dbUser.Adapt<User>();
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

            // Check if passwords do not match
            if (existingUser.HashedPassword != Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }

            // if user was not logged in from this location before
            if (!await _userLocationManager.UserWasLoggedInFromLocationAsync(userLocation))
            {
                if (existingUser.LastRequestDate.HasValue && existingUser.LastRequestDate.Value.AddMinutes(15) < DateTime.Now)
                {
                    //TODO: Handle multiple logins in an attempt to generate more OTPS
                    //Maybe just return null or throw error
                }
                existingUser = await _userProvider.UpdateUserLoginNewLocation(existingUser.ID);
                var hotp = Security.GetHotp(existingUser.SecretKey, existingUser.Counter);
                if (hotp != null)
                {
                    var loginFromAnotherLocationEmail = _messageProvider.GetLoginAttemptMessage(existingUser.Email, hotp);
                    await _messageService.SendMessageAsync(loginFromAnotherLocationEmail);
                }
                await _userLocationManager.LogLocationAsync(userLocation);
                throw new Required2FAException();
            }

            // login success 
            existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);
            _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");
            return existingUser.Adapt<User>();
        }

        /// <inheritdoc/>
        public async Task<User> LoginWithVerificationCodeAsync(UserLogin userLogin, UserLocation userLocation)
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

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] has been locked");
                throw new AccountLockedException();
            }


            // check if given otp password is valid
            if (!Security.VerifyHotp(userLogin.Password, existingUser))
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing with 2 step");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }

            // login success 
            existingUser = await _userProvider.UpdateUserLoginSuccessWithVerificationCode(existingUser.ID);
            userLocation.Successful = true;
            userLocation.UserID = existingUser.ID;
            await _userLocationManager.LogLocationAsync(userLocation);
            _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");

            return existingUser;
        }
        /// <summary>
        /// Checks whether the given password has been breached,
        /// by calling the leakedpassword provider.
        /// </summary>
        /// <param name="password">The password to check is breached</param>
        /// <returns>True if breached, false otherwise</returns>
        private async Task<bool> CheckPasswordLeakedForUser(string password)
        {
            var stringBytes = Encoding.UTF8.GetBytes(password);
            var hashedBytes = SHA1.HashData(stringBytes);
            var hashedPassword = Convert.ToHexString(hashedBytes);

            // Check if password has been leaked
            return await _leakedPasswordProvider.GetIsPasswordLeakedAsync(hashedPassword);
        }
    }
}



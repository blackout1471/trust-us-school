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
        private readonly IMessageManager _messageManager;
        private readonly IUserLocationManager _userLocationManager;
        private readonly ILeakedPasswordProvider _leakedPasswordProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserManager> _logger;
        public UserManager(IConfiguration configuration, IUserProvider userProvider, IMessageManager messageManager, ILogger<UserManager> logger, IUserLocationManager userLocationManager, ILeakedPasswordProvider leakedPasswordProvider)
        {
            _configuration = configuration;
            _userProvider = userProvider;
            _userLocationManager = userLocationManager;
            _leakedPasswordProvider = leakedPasswordProvider;
            _logger = logger;
            _messageManager = messageManager;
        }

        /// <inheritdoc/>
        public async Task<bool> CreateUserAsync(UserCreate userCreate, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new IpBlockedException();

            // Check if password has been leaked
            if (await CheckPasswordLeakedForUserAsync(userCreate.Password))
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

            toCreateDbUser.Salt = Security.GetSalt(32);
            toCreateDbUser.HashedPassword = Security.GetEncryptedAndSaltedPassword(userCreate.Password, toCreateDbUser.Salt, _configuration["Pepper"]);
            //TODO: Make a random counter start
            toCreateDbUser.Counter = 0;
            toCreateDbUser.SecretKey = Security.GetHmacKey();

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);
            _logger.LogInformation($"User[{userCreate.Email}] has been created");

            userLocation.UserID = createdUser.ID;
            userLocation.Successful = true;
            await _userLocationManager.LogLocationAsync(userLocation);
            if (!await _messageManager.SendRegistrationMessageAsync(createdUser.Email, createdUser.SecretKey))
                throw new SendMessageIssueException();

            return true;
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

            // Check if user is locked or not verified yet
            await IsUserLockedAsync(userLocation, existingUser);
            await IsUserNotVerifiedAsync(userLocation, existingUser);

            // Check if passwords do not match
            if (existingUser.HashedPassword != Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt, _configuration["Pepper"]))
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing");
                await _userProvider.UpdateUserFailedTriesAsync(existingUser.ID);
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
                existingUser = await _userProvider.UpdateUserLoginNewLocationAsync(existingUser.ID);
                var hotp = Security.GetHotp(existingUser.SecretKey, existingUser.Counter);
                if (hotp != null)
                {
                    if (!await _messageManager.SendLoginAttemptMessageAsync(existingUser.Email, hotp))
                        throw new SendMessageIssueException();
                    
                }
                await _userLocationManager.LogLocationAsync(userLocation);
                throw new Required2FAException();
            }

            // login success 
            existingUser = await _userProvider.UpdateUserLoginSuccessAsync(existingUser.ID);
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
            await IsUserLockedAsync(userLocation, existingUser);

            // check if given otp password is valid
            if (!IsVerificationCodeValid(userLogin.Password, existingUser))
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing with 2 step");
                await _userProvider.UpdateUserFailedTriesAsync(existingUser.ID);
                throw new UserIncorrectLoginException();
            }

            // login success 
            existingUser = await _userProvider.UpdateUserLoginSuccessWithVerificationCodeAsync(existingUser.ID);
            userLocation.Successful = true;
            userLocation.UserID = existingUser.ID;
            await _userLocationManager.LogLocationAsync(userLocation);
            _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");

            return existingUser;
        }

        /// <inheritdoc />
        public async Task<bool> VerifyUserRegistrationAsync(UserLogin userLogin, UserLocation userLocation)
        {
            if (await _userLocationManager.IsIPLockedAsync(userLocation.IP))
                throw new IpBlockedException();

            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);
            if (existingUser == null)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                return false;
            }

            // check whether user is locked, if they are. No need for further checks
            await IsUserLockedAsync(userLocation, existingUser);

            // Checks if otp password does not match
            if (userLogin.Password != existingUser.SecretKey)
            {
                // login failed
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{userLogin.Email}] failed at verifying registration");
                await _userProvider.UpdateUserFailedTriesAsync(existingUser.ID);
                throw new UserIncorrectLoginException();
            }

            // login success 
            existingUser = await _userProvider.UpdateUserLoginSuccessAsync(existingUser.ID);
            userLocation.Successful = true;
            userLocation.UserID = existingUser.ID;
            await _userLocationManager.LogLocationAsync(userLocation);
            _logger.LogInformation($"User[{userLogin.Email}] has verified registration");

            // Update verified status
            await _userProvider.UpdateUserToVerifiedAsync(existingUser.ID);

            return true;
        }

        /// <summary>
        /// Checks whether the given password has been breached,
        /// by calling the leakedpassword provider.
        /// </summary>
        /// <param name="password">The password to check is breached</param>
        /// <returns>True if breached, false otherwise</returns>
        private async Task<bool> CheckPasswordLeakedForUserAsync(string password)
        {
            var stringBytes = Encoding.UTF8.GetBytes(password);
            var hashedBytes = SHA1.HashData(stringBytes);
            var hashedPassword = Convert.ToHexString(hashedBytes);

            // Check if password has been leaked
            return await _leakedPasswordProvider.GetIsPasswordLeakedAsync(hashedPassword);
        }

        /// <summary>
        /// Checks whether the current given user is locked.
        /// Throws exceptions if that is the case.
        /// </summary>
        /// <param name="userLocation">The location to log</param>
        /// <param name="currentUser">The current existsting user.</param>
        /// <exception cref="AccountLockedException">The account is locked</exception>
        private async Task IsUserLockedAsync(UserLocation userLocation, DbUser currentUser)
        {
            // User is locked, no need for further checks
            if (currentUser.IsLocked)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{currentUser.Email}] has been locked");
                throw new AccountLockedException();
            }
        }

        /// <summary>
        /// Checks whether the current given user is not verified.
        /// Throws exception if that is the case.
        /// </summary>
        /// <param name="userLocation">The location to log</param>
        /// <param name="currentUser">The current user to handle.</param>
        /// <exception cref="AccountLockedException">The account is locked</exception>
        private async Task IsUserNotVerifiedAsync(UserLocation userLocation, DbUser currentUser)
        {
            if (!currentUser.IsVerified)
            {
                await _userLocationManager.LogLocationAsync(userLocation);
                _logger.LogWarning($"User[{currentUser.Email}] has tried to login to an un-verified account");
                throw new AccountIsNotVerifiedException();
            }
        }

        /// <summary>
        /// Verifies the verificaton code
        /// </summary>
        /// <param name="verificationCode"> Verification code</param>
        /// <param name="user"> The Db user</param>
        /// <returns>Whether or not the verification code is valid</returns>
        private bool IsVerificationCodeValid(string verificationCode, DbUser user)
        {
            // TODO: Add in SP
            if (!user.LastRequestDate.HasValue || user.LastRequestDate.Value.AddMinutes(15) < DateTime.Now)
            {
                return false;
            }

            if (verificationCode != Security.GetHotp(user.SecretKey, user.Counter))
            {
                return false;
            }

            return true;
        }
    }
}



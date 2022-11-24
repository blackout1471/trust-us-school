using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Mapster;

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
            _userLocationManager = userLocationManager;
            _logger = logger;
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

            // Map from user create to db user
            var toCreateDbUser = userCreate.Adapt<DbUser>();

            toCreateDbUser.Salt = Security.GetSalt(50);
            toCreateDbUser.HashedPassword = Security.GetEncryptedAndSaltedPassword(userCreate.Password, toCreateDbUser.Salt);

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);
            _logger.LogInformation($"User[{userCreate.Email}] has been created");

            userLocation.UserID = createdUser.ID;
            userLocation.Successful = true;
            await _userLocationManager.LogLocationAsync(userLocation);

            // Map from db user to user
            return createdUser.Adapt<User>();
        }

        public async Task<User> GetUserByIDAsync(int ID)
        {
            var dbUser = await _userProvider.GetUserByIDAsync(ID);

            if (dbUser == null)
                return null;

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


            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // if user was not logged in from this location before
                if (!await _userLocationManager.UserWasLoggedInFromLocationAsync(userLocation))
                {
                    // send 2fa here
                    await _userLocationManager.LogLocationAsync(userLocation);
                    throw new Required2FAException();
                }
                else
                {
                    // login success 
                    existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);
                    _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");
                }

                // Map from db user to user
                return existingUser.Adapt<User>();
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

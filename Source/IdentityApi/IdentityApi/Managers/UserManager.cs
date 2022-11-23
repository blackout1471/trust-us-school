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
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserProvider userProvider, ILogger<UserManager> logger)
        {
            _userProvider = userProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate)
        {
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
        public async Task<User> LoginAsync(UserLogin userLogin)
        {
            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
                return null;

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                _logger.LogWarning($"User[{userLogin.Email}] has been locked");
                throw new AccountLockedException();
            }

            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // TODO: Add check for ip adresse here
                // if user was not logged in with this ip adress
                // Send 2FA here, then
                // throw error here with "Check email", 

                // else

                // login success 
                existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);
                _logger.LogInformation($"User[{userLogin.Email}] has been authorized and logged in");
                // TODO: Update 
                return existingUser;
            }
            else
            {
                // login failed
                _logger.LogWarning($"User[{userLogin.Email}] failed at authorizing");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }
        }
    }
}

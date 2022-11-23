using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly ILeakedPasswordProvider _leakedPasswordProvider;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserProvider userProvider, ILogger<UserManager> logger, ILeakedPasswordProvider leakedPasswordProvider)
        {
            _userProvider = userProvider;
            _leakedPasswordProvider = leakedPasswordProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate)
        {
            if (await CheckPasswordLeakedForUser(userCreate.Password))
            {
                _logger.LogWarning($"User[{userCreate.Email}] tried to register leaked password");
                throw new PasswordLeakedException();
            }

            // check if user exists
            if (await _userProvider.GetUserByEmailAsync(userCreate.Email) != null)
            {
                _logger.LogWarning($"User cannot be created because they already exists {userCreate.Email}");
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
                _logger.LogWarning($"Login attempt for locked user {userLogin.Email}");
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
                _logger.LogInformation($"User has logged in {userLogin.Email}");
                // TODO: Update 
                return existingUser;
            }
            else
            {
                // login failed
                _logger.LogWarning($"Login failed for user email {userLogin.Email}");
                await _userProvider.UpdateUserFailedTries(existingUser.ID);
                throw new UserIncorrectLoginException();
            }
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

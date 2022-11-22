using IdentityApi.DbModels;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly IUserLocationManager _userLocationManager;
        public UserManager(IUserProvider userProvider, IUserLocationManager userLocationManager)
        {
            _userProvider = userProvider;
            _userLocationManager = userLocationManager;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate)
        {
            if (await _userLocationManager.IsIPLocked("provide ip"))
                throw new Exception("login failed");

            // check if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userCreate.Email);

            // User already in use 
            if (existingUser != null)
            {
                // TODO: log

                throw new Exception("Email already in use");
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

            //_userLocationManager.LogLocation(/*Log with user id*/);

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
            try
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
            catch (Exception e)
            {
                // TODO: Add log

                throw e;
            }
        }

        /// <inheritdoc/>
        public async Task<User> LoginAsync(UserLogin userLogin)
        {
            if (await _userLocationManager.IsIPLocked("provide ip"))
                throw new Exception("login failed");

            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
            {
                return null;
            }

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                // TODO: log
                throw new Exception("Login failed, Account locked");
            }


            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // if user was not logged in from this location before
                if (!await _userLocationManager.UserWasLoggedInFromLocation(/*provide location*/new UserLocation()))
                {
                    // send 2fa here

                    throw new Exception("forbidden");
                }
                else
                {
                    // login success 
                    existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);


                    // TODO: Update 

                }
                return existingUser;
            }
            else
            {
                // TODO: log
                // login failed

                //await _userLocationManager.LogLocation()

                existingUser = await _userProvider.UpdateUserFailedTries(existingUser.ID);

                if (existingUser.IsLocked)
                {
                    throw new Exception("Login failed, Account locked");
                }

                // update tries, if tries >= 3 lock account  <- consider moving both to sp and returning dbuser
                throw new Exception("Login failed, username or password is incorrect");
            }
        }
    }
}

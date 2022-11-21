using IdentityApi.DbModels;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        public UserManager(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public async Task<User> CreateUser(UserCreate userCreate)
        {
            // check if user exists
            var existingUser = await _userProvider.GetUserByEmail(userCreate.Email);

            // User already in use 
            if(existingUser != null)
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

            var createdUser = await _userProvider.CreateUser(toCreateDbUser);

            return new User()
            {
                ID = createdUser.ID,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,  
                LastName = createdUser.LastName,
                PhoneNumber = createdUser.PhoneNumber
            };
        }

        public async Task<User> Login(UserLogin userLogin)
        {
            // checjs uf yser exuts
            var existingUser = await _userProvider.GetUserByEmail(userLogin.Email);

            if(existingUser == null)
            {
                return null;
            }

            // check if given password matches with the hashedpassword of the user
            if(existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // login success 

                return existingUser;
            }
            else
            {
                // login failed

                // TODO: log

                // update tries, if tries >= 3 lock account  <- consider moving both to sp and returning dbuser
                throw new Exception("Login failed, username or password is incorrect");
            }
        }
    }
}

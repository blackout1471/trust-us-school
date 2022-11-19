using IdentityApi.DbModels;
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
            // TODO: create mapper

            var toCreateDbUser = new DbUser()
            {
                Email = userCreate.Email,
                HashedPassword = userCreate.Password, // TODO: Replace with salted password
                Salt = "123" // TODO: Replace with the salt
            };

            var createdUser = await _userProvider.CreateUser(toCreateDbUser);

            return new User()
            {
                Id = createdUser.ID,
                Email = createdUser.Email
            };
        }

        public async Task<User> Login(UserLogin userLogin)
        {
            var existingUser = await _userProvider.GetUserByEmail(userLogin.Email);

            if(existingUser == null)
            {
                return null;
            }

            return new User()
            {
                Id = existingUser.ID,
                Email = existingUser.Email
            };
        }
    }
}

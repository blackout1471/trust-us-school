using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserManager
    {
        /// <summary>
        /// Creates user
        /// </summary>
        Task<User> CreateUser(UserCreate userCreate);

        /// <summary>
        /// Logs user in
        /// </summary>
        /// <returns>Logged in user</returns>
        Task<User> Login(UserLogin userLogin);
    }
}

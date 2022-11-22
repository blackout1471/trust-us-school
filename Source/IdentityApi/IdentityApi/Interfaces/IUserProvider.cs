using IdentityApi.DbModels;
using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserProvider
    {
        /// <summary>
        /// Creates user
        /// </summary>
        /// <returns>Created user</returns>
        Task<DbUser> CreateUserAsync(DbUser userCreate);

        /// <summary>
        /// Retrieves user based on email
        /// </summary>
        Task<DbUser> GetUserByEmailAsync(string email);

        /// <summary>
        /// Retrieves user based on id 
        /// </summary>
        Task<DbUser> GetUserByIDAsync(int userID);
    }
}

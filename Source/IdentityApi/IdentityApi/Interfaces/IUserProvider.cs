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
        Task<DbUser> CreateUser(DbUser userCreate);

        /// <summary>
        /// Retrieves user based on email
        /// </summary>
        Task<DbUser> GetUserByEmail(string email);

        /// <summary>
        /// Retrieves user based on id 
        /// </summary>
        Task<DbUser> GetUserByID(int userID);
    }
}

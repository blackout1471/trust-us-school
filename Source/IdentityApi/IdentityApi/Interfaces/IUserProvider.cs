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

        /// <summary>
        /// Updates the failed tries on the user 
        /// Locks account if tries are above the threshold
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserFailedTries(int userID);

        /// <summary>
        /// Updates user successful login
        /// Removes any failed tries if there were any
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginSuccess(int userID);

        /// <summary>
        /// Updates user successful login from new location
        /// Updates the last request date
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginNewLocation(int userID);

        /// <summary>
        /// Updates user successful login with verification code
        /// Removes the time the verification code is valid
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginSuccessWithVerificationCode(int userID);
    }
}

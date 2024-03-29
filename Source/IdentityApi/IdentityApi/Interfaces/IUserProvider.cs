﻿using IdentityApi.DbModels;
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
        Task<DbUser> UpdateUserFailedTriesAsync(int userID);

        /// <summary>
        /// Updates user successful login
        /// Removes any failed tries if there were any
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginSuccessAsync(int userID);

        /// <summary>
        /// Updates user successful login from new location
        /// Updates the last request date
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginNewLocationAsync(int userID);

        /// <summary>
        /// Updates user successful login with verification code
        /// Removes the time the verification code is valid
        /// </summary>
        /// <returns>Updated user</returns>
        Task<DbUser> UpdateUserLoginSuccessWithVerificationCodeAsync(int userID);

        /// <summary>
        /// Updates the status of the user in database to be verfied (IsVerfied = 1)
        /// </summary>
        /// <param name="userID">The userId to update.</param>
        Task UpdateUserToVerifiedAsync(int userID);

    }
}

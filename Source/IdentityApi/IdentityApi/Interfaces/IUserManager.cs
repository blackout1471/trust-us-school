﻿using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserManager
    {
        /// <summary>
        /// Creates user
        /// </summary>
        Task<User> CreateUserAsync(UserCreate userCreate, UserLocation userLocation);

        /// <summary>
        /// Logs user in
        /// </summary>
        /// <returns>Logged in user</returns>
        Task<User> LoginAsync(UserLogin userLogin, UserLocation userLocation);

        /// <summary>
        /// Logs user in with otp
        /// </summary>
        /// <returns>Logged in user</returns>
        Task<User> Login2FaAsync(UserLogin userLogin);

        /// <summary>
        /// Gets user based on id
        /// </summary>
        /// <param name="ID">ID of the requested user</param>
        /// <returns>User with matching ID</returns>
        Task<User> GetUserByIDAsync(int ID);
    }
}

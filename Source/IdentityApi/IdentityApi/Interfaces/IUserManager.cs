using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserManager
    {
        /// <summary>
        /// Creates an user, and sends a verfication email to the user.
        /// </summary>
        /// <returns>True if user has been created, false otherwise</returns>
        Task<bool> CreateUserAsync(UserCreate userCreate, UserLocation userLocation);

        /// <summary>
        /// Logs user in
        /// </summary>
        /// <returns>Logged in user</returns>
        Task<User> LoginAsync(UserLogin userLogin, UserLocation userLocation);

        /// <summary>
        /// Logs user in with otp
        /// </summary>
        /// <returns>Logged in user</returns>
        Task<User> LoginWithVerificationCodeAsync(UserLogin userLogin, UserLocation userLocation);

        /// <summary>
        /// Verifies the registered user by using an one time password.
        /// </summary>
        /// <param name="userLogin">The user credentials, password is one time password.</param>
        /// <returns>True if user is verified, false otherwise.</returns>
        Task<bool> VerifyUserRegistrationAsync(UserLogin userLogin, UserLocation userlocation);

        /// <summary>
        /// Gets user based on id
        /// </summary>
        /// <param name="ID">ID of the requested user</param>
        /// <returns>User with matching ID</returns>
        Task<User> GetUserByIDAsync(int ID);
    }
}

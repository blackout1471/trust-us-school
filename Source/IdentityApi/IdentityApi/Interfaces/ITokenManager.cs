using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface ITokenManager
    {
        /// <summary>
        /// Generates a token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate token from.</param>
        /// <returns>The generated token.</returns>
        public UserToken GenerateUserToken(User user);

        /// <summary>
        /// Validates a given token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if valid, false if not.</returns>
        public bool ValidateToken(string token);

        /// <summary>
        /// Converts a token string to UserToken model.
        /// </summary>
        /// <param name="token">The token to convert.</param>
        /// <returns>The converted token.</returns>
        public UserToken GetUserTokenFromToken(string token);
    }
}

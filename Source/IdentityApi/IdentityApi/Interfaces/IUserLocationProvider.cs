using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationProvider
    {
        /// <summary>
        /// Checks if ip adress is blacklisted 
        /// </summary>
        /// <returns>True if ip is blocked</returns>
        public Task<bool> IsIPLockedAsync(string ipAddress);

        /// <summary>
        /// Saves user location
        /// </summary>
        /// <returns>Saved location</returns>
        public Task<UserLocation> LogLocationAsync(UserLocation userLocation);

        /// <summary>
        /// Checks whether or not user was logged in from this location
        /// </summary>
        /// <returns>True if user was logged in from location</returns>
        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation);
    }
}

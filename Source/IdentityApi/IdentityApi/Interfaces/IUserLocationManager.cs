using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationManager
    {
        /// <summary>
        /// Checks if ip adress is blacklisted 
        /// </summary>
        /// <returns>True if account is blocked, false if its not</returns>
        public Task<bool> IsIPLockedAsync(string ipAddress);

        /// <summary>
        /// Saves user location
        /// </summary>
        /// <returns>Saved userlocation</returns>
        public Task<UserLocation> LogLocationAsync(UserLocation location);

        /// <summary>
        /// Checks whether or not user was logged in from this location
        /// </summary>
        /// <returns>True if user was logged in from this user location before</returns>
        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation);
    }
}

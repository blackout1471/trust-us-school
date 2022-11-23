using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationManager
    {
        /// <summary>
        /// Checks if ip adress is blacklisted 
        /// </summary>
        public Task<bool> IsIPLockedAsync(string ipAddress);

        /// <summary>
        /// Saves user location
        /// </summary>
        public Task<UserLocation> LogLocationAsync(UserLocation location);

        /// <summary>
        /// Checks whether or not user was logged in from this location
        /// </summary>
        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation);
    }
}

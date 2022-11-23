using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationManager
    {
        public Task<bool> IsIPLockedAsync(string ipAddress);
        public Task<UserLocation> LogLocationAsync(UserLocation location);
        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation);
    }
}

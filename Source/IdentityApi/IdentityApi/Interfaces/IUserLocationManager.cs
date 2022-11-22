using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationManager
    {
        public Task<bool> IsIPLocked(string ipAddress);
        public Task<UserLocation> LogLocation(UserLocation location);
        public Task<bool> UserWasLoggedInFromLocation(UserLocation userLocation);
    }
}

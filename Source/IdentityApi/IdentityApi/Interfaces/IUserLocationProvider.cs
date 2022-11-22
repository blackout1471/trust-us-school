using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationProvider
    {
        public Task<bool> IsIPLocked(string ipAddress);
        public Task<UserLocation> LogLocation(UserLocation userLocation);
        public Task<bool> UserWasLoggedInFromLocation(UserLocation userLocation);
    }
}

using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserLocationProvider
    {
        public Task<bool> IsIPLockedAsync(string ipAddress);
        public Task<UserLocation> LogLocationAsync(UserLocation userLocation);
        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation);
    }
}

using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Models;

namespace IdentityApi.Managers
{
    public class UserLocationManager : IUserLocationManager
    {
        private readonly IUserLocationProvider _userLocationProvider;

        public UserLocationManager(IUserLocationProvider userLocationProvider)
        {
            _userLocationProvider = userLocationProvider;
        }

        public Task<bool> IsIPLockedAsync(string ipAddress)
        {
            return _userLocationProvider.IsIPLockedAsync(ipAddress);
        }

        public Task<UserLocation> LogLocationAsync(UserLocation location)
        {
            location.UserAgent = RegexHelper.TryToGetBrowserWithoutVersion(location.UserAgent);

            return _userLocationProvider.LogLocationAsync(location);
        }

        public Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation)
        {
            userLocation.UserAgent = RegexHelper.TryToGetBrowserWithoutVersion(userLocation.UserAgent);

            return _userLocationProvider.UserWasLoggedInFromLocationAsync(userLocation);
        }
    }
}

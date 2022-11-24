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

        /// <inheritdoc/>
        public async Task<bool> IsIPLockedAsync(string ipAddress)
        {
            return await _userLocationProvider.IsIPLockedAsync(ipAddress);
        }

        /// <inheritdoc/>
        public async Task<UserLocation> LogLocationAsync(UserLocation location)
        {
            location.UserAgent = RegexHelper.TryToGetBrowserWithoutVersion(location.UserAgent);

            return await _userLocationProvider.LogLocationAsync(location);
        }

        /// <inheritdoc/>
        public async Task<bool> UserWasLoggedInFromLocationAsync(UserLocation userLocation)
        {
            userLocation.UserAgent = RegexHelper.TryToGetBrowserWithoutVersion(userLocation.UserAgent);

            return await _userLocationProvider.UserWasLoggedInFromLocationAsync(userLocation);
        }
    }
}

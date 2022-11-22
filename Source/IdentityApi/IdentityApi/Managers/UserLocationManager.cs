using IdentityApi.Interfaces;
using IdentityApi.Models;
using System.Text.RegularExpressions;

namespace IdentityApi.Managers
{
    public class UserLocationManager : IUserLocationManager
    {
        private readonly IUserLocationProvider _userLocationProvider;

        public UserLocationManager(IUserLocationProvider userLocationProvider)
        {
            _userLocationProvider = userLocationProvider;
        }

        public Task<bool> IsIPLocked(string ipAddress)
        {
            return _userLocationProvider.IsIPLocked(ipAddress);
        }

        public Task<UserLocation> LogLocation(UserLocation location)
        {
            var match = Regex.Match(location.UserAgent, "(?i)(firefox|msie|chrome|safari)[/\\s]([\\d.]+)");

            if (match.Success)
            {
                // Try removing the browser version
                var browser = match.Captures.First().Value.Split('/')[0];
                location.UserAgent = location.UserAgent.Replace(match.Captures.First().Value, browser);
            }

            return _userLocationProvider.LogLocation(location);
        }

        public Task<bool> UserWasLoggedInFromLocation(UserLocation userLocation)
        {
            return _userLocationProvider.UserWasLoggedInFromLocation(userLocation);
        }
    }
}

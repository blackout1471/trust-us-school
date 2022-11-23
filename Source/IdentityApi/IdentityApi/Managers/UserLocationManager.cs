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
            location.UserAgent = TryToGetBrowserWithoutVersion(location.UserAgent);

            return _userLocationProvider.LogLocation(location);
        }

        public string TryToGetBrowserWithoutVersion(string userAgent)
        {
            try
            {
                //var match = Regex.Match(userAgent, "(?i)(firefox|msie|chrome|safari)[/\\s]([\\d.]+)");
                Match match = null;

                // removes all versions
                while ((match = Regex.Match(userAgent, "(?i)(firefox|msie|chrome|safari)[/\\s]([\\d.]+)"))?.Success ?? false)
                {
                    foreach (Capture capture in match.Captures)
                    {
                        // Try removing the browser version
                        var browser = capture.Value.Split('/')[0];
                        userAgent = userAgent.Replace(capture.Value, browser);
                    }

                }
                return userAgent;
            }
            catch (Exception e)
            {
                // Todo: log
            }
            return userAgent;
        }

        public Task<bool> UserWasLoggedInFromLocation(UserLocation userLocation)
        {
            userLocation.UserAgent = TryToGetBrowserWithoutVersion(userLocation.UserAgent);

            return _userLocationProvider.UserWasLoggedInFromLocation(userLocation);
        }
    }
}

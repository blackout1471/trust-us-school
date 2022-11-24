using System.Text.RegularExpressions;

namespace IdentityApi.Helpers
{
    public static class RegexHelper
    {
        /// <summary>
        /// Tries to remove browser version from browser user agent
        /// </summary>
        public static string TryToGetBrowserWithoutVersion(string browser)
        {
            try
            {
                Match match = null;

                // removes all versions
                while ((match = Regex.Match(browser, "(?i)(firefox|msie|chrome|safari|edg)[/\\s]([\\d.]+)"))?.Success ?? false)
                {
                    foreach (Capture capture in match.Captures)
                    {
                        // Try removing the browser version
                        var spltBrowser = capture.Value.Split('/')[0];
                        browser = browser.Replace(capture.Value, spltBrowser);
                    }
                }
                return browser;
            }
            catch (Exception e)
            {
                // Todo: log
            }
            return browser;
        }
    }
}

using IdentityApi.Helpers;
using IdentityApi.Managers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApiUnitTest.Utiltiy
{
    public class RegexHelperShould
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36")] // chrome
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0")] // firefox
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.56")] // edge
        [InlineData("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko")] // ie 
        [InlineData("Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0")] // tor
        public void ExpectNoException_WhenRemovingVersion_OnRemoveVersion(string userAgent)
        {
            List<string> expectedResults = new List<string>
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome Safari",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome Safari Edg",
                "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko",
                "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox"
            };

            var actual = RegexHelper.TryToGetBrowserWithoutVersion(userAgent);

            Assert.Contains(expectedResults, (x) => { return x.Contains(actual); });
        }

    }
}

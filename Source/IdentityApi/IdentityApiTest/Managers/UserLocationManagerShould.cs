using FakeItEasy;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApiUnitTest.Managers
{
    public class UserLocationManagerShould
    {
        [Fact]
        public async void ExpectTrue_WhenIPBlocked_IPLock()
        {
            // arrange
            bool expected = true;
            string evilIP = "123.123.123.123";
            var userLocationProvider = A.Fake<IUserLocationProvider>();

            A.CallTo(() => userLocationProvider.IsIPLockedAsync(evilIP)).Returns(true);

            // act
            var userLocationManager = new UserLocationManager(userLocationProvider);
            var actual = await userLocationProvider.IsIPLockedAsync(evilIP);

            // assert
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async void ExpectTrue_WhenUserHasBeenLoggedIn_IPCheck()
        {
            // arrange
            var userLocation = GetUserLocation();

            bool expected = true;
            var userLocationProvider = A.Fake<IUserLocationProvider>();

            A.CallTo(() => userLocationProvider.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(true);

            // act
            var userLocationManager = new UserLocationManager(userLocationProvider);
            var actual = await userLocationProvider.UserWasLoggedInFromLocationAsync(userLocation);

            // assert
            Assert.Equal(expected, actual);
        }

        private UserLocation GetUserLocation()
        {
            return new UserLocation()
            {
                UserID = 0,
                UserAgent = "123",
                IP = "10.0.0.1"
            };
        }

    }
}

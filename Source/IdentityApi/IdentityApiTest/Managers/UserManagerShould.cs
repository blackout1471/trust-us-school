﻿using FakeItEasy;
using IdentityApi.DbModels;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;

namespace IdentityApiUnitTest.Managers
{
    public class UserManagerShould
    {
        [Fact]
        public void ExpectUserDetails_WhenLoggingIn_Login()
        {
            // Arrange 
            var expected = GetUser();
            var userLogin = GetUserLogin();

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();

            A.CallTo(() => userProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());
            A.CallTo(() => userProvider.UpdateUserLoginSuccess(expected.ID)).Returns(GetDbUser());
            A.CallTo(() => userLocation.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(true);
            // Act
            var userManager = new UserManager(userProvider, userLocation);
            var actual = userManager.LoginAsync(userLogin, GetUserLocation()).Result;

            // Assert
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.ID, actual.ID);
        }

        [Fact]
        public void ThrowsException_WhenPasswordIsWrong_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();
            userLogin.Password = "PasswordThatIsWrong";

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();
            A.CallTo(() => userProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());

            // Act
            var userManager = new UserManager(userProvider, userLocation);


            // Assert
            Assert.ThrowsAny<Exception>(() => userManager.LoginAsync(userLogin, GetUserLocation()).Result);
        }

        [Fact]
        public void ThrowsException_WhenUserExists_Register()
        {
            // Arrange
            var expected = GetUser();
            var userCreate = GetUserCreate();

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();
            A.CallTo(() => userProvider.CreateUserAsync(A<DbUser>.Ignored)).Returns(GetDbUser());

            // Act
            var userManager = new UserManager(userProvider, userLocation);

            // Assert
            Assert.ThrowsAny<Exception>(() => userManager.CreateUserAsync(userCreate, GetUserLocation()).Result);
        }


        [Fact]
        public void ThrowsException_WhenIPBlocked_Register()
        {
            // Arrange
            var userCreate = GetUserCreate();

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();
            A.CallTo(() => userProvider.CreateUserAsync(A<DbUser>.Ignored)).Returns(GetDbUser());
            A.CallTo(() => userLocation.IsIPLockedAsync(A<string>.Ignored)).Returns(true);            

            // Act
            var userManager = new UserManager(userProvider, userLocation);

            // Assert
            Assert.ThrowsAny<Exception>(() => userManager.CreateUserAsync(userCreate, GetUserLocation()).Result);
        }

        [Fact]
        public void ThrowsException_WhenIPBlocked_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();

            A.CallTo(() => userLocation.IsIPLockedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var userManager = new UserManager(userProvider, userLocation);

            // Assert
            Assert.ThrowsAny<Exception>(() => userManager.LoginAsync(userLogin, GetUserLocation()).Result);
        }

        [Fact]
        public void ThrowsException_WhenUserNotLoggedInLoctaion_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();

            var userProvider = A.Fake<IUserProvider>();
            var userLocation = A.Fake<IUserLocationManager>();

            A.CallTo(() => userLocation.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => userLocation.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(false);

            // Act
            var userManager = new UserManager(userProvider, userLocation);

            // Assert
            Assert.ThrowsAny<Exception>(() => userManager.LoginAsync(userLogin, GetUserLocation()).Result);
        }


        private User GetUser()
        {
            return new User()
            {
                ID = 1,
                Email = "a@b.com",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
        }

        private DbUser GetDbUser()
        {
            return new DbUser()
            {
                ID = 1,
                Email = "a@b.com",
                HashedPassword = "C123469DB6AEE601787C5974D80A7D7223486F1523B00803E82EF60CD5EB27918C1CA9F77AE8B61E73CF944CE82B469491CEEDEDFE89C936DAE3220F6C7A78B9",
                Salt = "CVkTFhIFerV0uxOMbZ0fFlE4HFVrwOs2PW5kkPEvzSoFcVJkNP",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
        }


        private UserLogin GetUserLogin()
        {
            return new UserLogin()
            {
                Email = "a@b.com",
                Password = "tron"
            };
        }

        private UserCreate GetUserCreate()
        {
            return new UserCreate()
            {
                Email = "a@b.com",
                Password = "abc",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
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

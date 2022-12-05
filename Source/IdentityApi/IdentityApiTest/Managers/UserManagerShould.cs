using FakeItEasy;
using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityApiUnitTest.Managers
{
    public class UserManagerShould
    {
        private readonly ILogger<UserManager> _fakeLogger;
        private readonly IUserProvider _fakeUserProvider;
        private readonly ILeakedPasswordProvider _fakeLeakedPasswordProvider;
        private readonly IUserLocationManager _fakeLocationManager;
        private readonly IConfiguration _configuration;
        private readonly IMessageManager _messageManager;
        private readonly UserManager _userManager;

        public UserManagerShould()
        {
            _fakeLogger = A.Fake<ILogger<UserManager>>();
            _fakeUserProvider = A.Fake<IUserProvider>();
            _fakeLeakedPasswordProvider = A.Fake<ILeakedPasswordProvider>();
            _fakeLocationManager = A.Fake<IUserLocationManager>();
            _messageManager = A.Fake<IMessageManager>();
            _configuration = A.Fake<IConfiguration>();

            _userManager = new UserManager(
                _configuration,
                _fakeUserProvider,
                _messageManager,
                _fakeLogger,
                _fakeLocationManager, 
                _fakeLeakedPasswordProvider
            );
        }

        [Fact]
        public async Task ExpectUserDetails_WhenLoggingIn_Login()
        {
            // Arrange 
            var expected = GetUser();
            var userLogin = GetUserLogin();
            
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginSuccessAsync(expected.ID)).Returns(GetDbUser());
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(true);
            A.CallTo(() => _configuration[A<string>.Ignored]).Returns("cENgCHeYQSv/FYL7tJwIQT7BIYcxI8b8uBe9oKfFzes=");
            // Act
            var actual = await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.ID, actual.ID);
        }

        [Fact]
        public async Task ExpectUserDetails_WhenLoggingIn_LoginWithVerificationCode()
        {
            // Arrange 
            var expected = GetUser();
            var userLogin = GetUserLoginVerification();

            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginSuccessWithVerificationCodeAsync(expected.ID)).Returns(GetDbUser());
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(true);

            // Act
            var actual = await _userManager.LoginWithVerificationCodeAsync(userLogin, GetUserLocation());

            // Assert
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.ID, actual.ID);
        }

        [Fact]
        public async Task ThrowsUserIncorrectLoginException_WhenPasswordIsWrong_Login()
        {
            // Arrange
            var userLogin = GetUserLogin();
            userLogin.Password = "PasswordThatIsWrong";
            var dbUser = GetDbUser();

            var userLocation = A.Fake<IUserLocationManager>();
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());

            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<UserIncorrectLoginException>(func);
        }

        [Fact]
        public async Task ThrowsUserIncorrectLoginException_WhenPasswordIsWrong_LoginWithVerificationCode()
        {
            // Arrange
            var userLogin = GetUserLogin();
            userLogin.Password = "PasswordThatIsWrong";
            var dbUser = GetDbUser();

            var userLocation = A.Fake<IUserLocationManager>();
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());

            // Act
            var func = async () => await _userManager.LoginWithVerificationCodeAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<UserIncorrectLoginException>(func);
        }

        [Fact]
        public async Task ThrowsAccountLockedException_WhenPasswordIsWrong_Login()
        {
            // Arrange
            var userLogin = GetUserLogin();
            var dbUser = GetDbUser();
            dbUser.IsLocked = true;

            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(dbUser);

            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<AccountLockedException>(func);
        }

        [Fact]
        public async Task ThrowsUserAlreadyExistsException_WhenUserExists_Register()
        {
            // Arrange
            var userCreate = GetUserCreate();
            
            A.CallTo(() => _fakeUserProvider.CreateUserAsync(A<DbUser>.Ignored)).Returns(GetDbUser());

            // Act

            var func = async () => await _userManager.CreateUserAsync(userCreate, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<UserAlreadyExistsException>(func);
        }

        [Fact]
        public async Task ThrowPasswordLeakedException_WhenPasswordHasBeenLeaked_Register()
        {
            // Arrange
            var userCreate = GetUserCreate();

            A.CallTo(() => _fakeLeakedPasswordProvider.GetIsPasswordLeakedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var func = async () => await _userManager.CreateUserAsync(userCreate, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<PasswordLeakedException>(func);
        }

        [Fact]
        public async void ThrowsIpBlockedException_WhenIPBlocked_Register()
        {
            // Arrange
            var userCreate = GetUserCreate();

            var userProvider = A.Fake<IUserProvider>();
            A.CallTo(() => userProvider.CreateUserAsync(A<DbUser>.Ignored)).Returns(GetDbUser());
            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var func = async () => await _userManager.CreateUserAsync(userCreate, GetUserLocation());

            // Assert
            await Assert.ThrowsAnyAsync<IpBlockedException>(func);
        }

        [Fact]
        public async void ThrowsIpBlockedException_WhenIPBlocked_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAnyAsync<IpBlockedException>(func);
        }

        [Fact]
        public async void ThrowsIpBlockedException_WhenIPBlocked_LoginWithVerificationCode()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLoginVerification();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var func = async () => await _userManager.LoginWithVerificationCodeAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAnyAsync<IpBlockedException>(func);
        }

        [Fact]
        public async void ThrowsException_WhenUserNotLoggedInLocation_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns(GetDbUser());
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginNewLocationAsync(A<int>.Ignored)).Returns(GetDbUser());
            A.CallTo(() => _configuration[A<string>.Ignored]).Returns("cENgCHeYQSv/FYL7tJwIQT7BIYcxI8b8uBe9oKfFzes=");
            A.CallTo(() => _messageManager.SendLoginAttemptMessageAsync(A<string>.Ignored, A<string>.Ignored)).Returns(true);
            

            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<Required2FAException>(func);
        }

        [Fact]
        public async void ThrowsAccountIsNotVerifiedException_WhenUserIsNotVerified_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();
            var dbUser = GetDbUser();
            dbUser.IsVerified = false;

            var userProvider = A.Fake<IUserProvider>();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored))
                .Returns(true);

            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns(dbUser);
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginNewLocationAsync(A<int>.Ignored)).Returns(dbUser);
            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<AccountIsNotVerifiedException>(func);
        }

        [Fact]
        public async void ThrowsIpBlockedException_WhenUserIsBlocked_VerifyUserRegistrationAsync()
        {
            // Arrange
            var userLogin = GetUserLogin();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(true);
            
            // Act
            var func = async () => await _userManager.VerifyUserRegistrationAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<IpBlockedException>(func);
        }

        [Fact]
        public async void ExpectFalse_WhenUserDoNotExist_VerifyUserRegistrationAsync()
        {
            // Arrange
            var userLogin = GetUserLogin();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns<DbUser>(null);

            // Act
            var actual = await _userManager.VerifyUserRegistrationAsync(userLogin, GetUserLocation());

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public async void ThrowUserIsLockedException_WhenUserIsLocked_VerifyUserRegistrationAsync()
        {
            // Arrange
            var userLogin = GetUserLogin();
            var dbUser = GetDbUser();
            dbUser.IsLocked = true;

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns<DbUser>(dbUser);

            // Act
            var func = async () => await _userManager.VerifyUserRegistrationAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<AccountLockedException>(func);
        }

        [Fact]
        public async void ThrowUserIncorrectLoginException_WhenPasswordsDoNotMatch_VerifyUserRegistrationAsync()
        {
            // Arrange
            var userLogin = GetUserLogin();
            var dbUser = GetDbUser();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns<DbUser>(dbUser);

            // Act
            var func = async () => await _userManager.VerifyUserRegistrationAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<UserIncorrectLoginException>(func);
        }

        [Fact]
        public async void ThrowsSendMessageIssueException_WhenSendRegistrationMessageReturnsFalse_Register()
        {
            // Arrange
            var fakeUserCreate = A.Fake<UserCreate>();
            fakeUserCreate.Password = "";
            var fakeUserLoc = A.Fake<UserLocation>();

            A.CallTo(() => _fakeLeakedPasswordProvider.GetIsPasswordLeakedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns<DbUser>(null);
            A.CallTo(() => _messageManager.SendRegistrationMessageAsync(A<string>.Ignored, A<string>.Ignored)).Returns(false);
            // Act
            var func = async () => await _userManager.CreateUserAsync(fakeUserCreate, fakeUserLoc);
            // Assert
            await Assert.ThrowsAsync<SendMessageIssueException>(func);
        }

        [Fact]
        public async void ThrowsSendMessageIssueException_WhenSendLoginAttemptMessageReturnsFalse_Login()
        {
            // Arrange
            var dbUser = GetDbUser();
            var userLogin = GetUserLogin();
            var fakeUserLoc = A.Fake<UserLocation>();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns<DbUser>(dbUser);
            A.CallTo(() => _messageManager.SendLoginAttemptMessageAsync(A<string>.Ignored, A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginNewLocationAsync(A<int>.Ignored)).Returns(dbUser);
            A.CallTo(() => _configuration[A<string>.Ignored]).Returns("cENgCHeYQSv/FYL7tJwIQT7BIYcxI8b8uBe9oKfFzes=");
            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, fakeUserLoc);
            // Assert
            await Assert.ThrowsAsync<SendMessageIssueException>(func);
        }


        private User GetUser()
        {
            return new User()
            {
                ID = 1,
                Email = "a@b.com",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578",
                IsVerified = true,
            };
        }

        private DbUser GetDbUser()
        {
            return new DbUser()
            {
                ID = 1,
                Email = "a@b.com",
                HashedPassword = "eG9Cgwq22HalElRHffdS+VRpovkQE7GVTjBo2N5jqHk7Sm6q0KdJRotjjoGupudBVgAUJrHD+DBmYU4o43n9rw==",
                Salt = "rIVLXrD6SaBRQxLv7zkYOQrLbfe3s2xkdupQvYAEBAo=",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578",
                SecretKey = "ABCDE123",
                LastRequestDate = DateTime.Now,
                Counter = 33,
                IsVerified = true,
            };
        }


        private UserLogin GetUserLogin()
        {
            return new UserLogin()
            {
                Email = "a@b.com",
                Password = "verysecurepassword"
            };
        }
        private UserLogin GetUserLoginVerification()
        {
            return new UserLogin()
            {
                Email = "a@b.com",
                Password = "36DA913AAFC687A11B5BF11313A493AC18D4A56DB7B6E4B24689F07E1ECD36A5FAE2D293B98A622BBB2494CECE8A35F98D1719483DCC226EFEC9DE86274A9B0E"
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

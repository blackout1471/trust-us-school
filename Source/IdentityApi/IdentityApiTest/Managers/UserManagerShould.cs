using FakeItEasy;
using FakeItEasy.Configuration;
using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;
using IdentityApi.Providers;
using MessageService.MessageServices;
using MessageService.Providers;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IdentityApiUnitTest.Managers
{
    public class UserManagerShould
    {
        private readonly ILogger<UserManager> _fakeLogger;
        private readonly IUserProvider _fakeUserProvider;
        private readonly ILeakedPasswordProvider _fakeLeakedPasswordProvider;
        private readonly IUserLocationManager _fakeLocationManager;
        private readonly IMessageService _messageService;
        private readonly IMessageProvider _messageProvider;
        private readonly UserManager _userManager;

        public UserManagerShould()
        {
            _fakeLogger = A.Fake<ILogger<UserManager>>();
            _fakeUserProvider = A.Fake<IUserProvider>();
            _fakeLeakedPasswordProvider = A.Fake<ILeakedPasswordProvider>();
            _fakeLocationManager = A.Fake<IUserLocationManager>();
            _messageService = A.Fake<IMessageService>();
            _messageProvider = A.Fake<IMessageProvider>();

            _userManager = new UserManager(
                _fakeUserProvider,
                _messageService,
                _messageProvider,
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
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginSuccess(expected.ID)).Returns(GetDbUser());
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(true);

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
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginSuccess(expected.ID)).Returns(GetDbUser());
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

            var userProvider = A.Fake<IUserProvider>();

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

            var userProvider = A.Fake<IUserProvider>();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(true);

            // Act
            var func = async () => await _userManager.LoginWithVerificationCodeAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAnyAsync<IpBlockedException>(func);
        }

        [Fact]
        public async void ThrowsException_WhenUserNotLoggedInLoctaion_Login()
        {
            // Arrange
            var expected = GetUser();
            var userLogin = GetUserLogin();

            var userProvider = A.Fake<IUserProvider>();

            A.CallTo(() => _fakeLocationManager.IsIPLockedAsync(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeLocationManager.UserWasLoggedInFromLocationAsync(A<UserLocation>.Ignored)).Returns(false);
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(A<string>.Ignored)).Returns(GetDbUser());
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginNewLocation(A<int>.Ignored)).Returns(GetDbUser());
            // Act
            var func = async () => await _userManager.LoginAsync(userLogin, GetUserLocation());

            // Assert
            await Assert.ThrowsAsync<Required2FAException>(func);
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
                PhoneNumber = "13246578",
                SecretKey = "ABCDE123",
                Counter = 33
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

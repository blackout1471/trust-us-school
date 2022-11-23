using FakeItEasy;
using IdentityApi.DbModels;
using IdentityApi.Exceptions;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;
using Microsoft.Extensions.Logging;

namespace IdentityApiUnitTest.Managers
{
    public class UserManagerShould
    {
        private readonly ILogger<UserManager> _fakeLogger;
        private readonly IUserProvider _fakeUserProvider;
        private readonly UserManager _userManager;

        public UserManagerShould()
        {
            _fakeLogger = A.Fake<ILogger<UserManager>>();
            _fakeUserProvider = A.Fake<IUserProvider>();

            _userManager = new UserManager(_fakeUserProvider, _fakeLogger);
        }

        [Fact]
        public async Task ExpectUserDetails_WhenLoggingIn_Login()
        {
            // Arrange 
            var expected = GetUser();
            var userLogin = GetUserLogin();
            
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(GetDbUser());
            A.CallTo(() => _fakeUserProvider.UpdateUserLoginSuccess(expected.ID)).Returns(GetDbUser());
            
            // Act
            var actual = await _userManager.LoginAsync(userLogin);

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
            
            A.CallTo(() => _fakeUserProvider.GetUserByEmailAsync(userLogin.Email)).Returns(dbUser);

            // Act
            var func = async () => await _userManager.LoginAsync(userLogin);

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
            var func = async () => await _userManager.LoginAsync(userLogin);

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
            var func = async () => await _userManager.CreateUserAsync(userCreate);

            // Assert
            await Assert.ThrowsAsync<UserAlreadyExistsException>(func);
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

    }
}

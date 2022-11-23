using FakeItEasy;
using IdentityApi.Controllers;
using IdentityApi.Exceptions;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApiUnitTest.Controllers
{
    public class UserControllerShould
    {
        private readonly IUserManager _fakeUserManager;
        private readonly ITokenManager _fakeTokenManager;

        private readonly UserController _userController;

        public UserControllerShould()
        {
            _fakeUserManager = A.Fake<IUserManager>();
            _fakeTokenManager = A.Fake<ITokenManager>();

            _userController = new UserController(_fakeUserManager, _fakeTokenManager);
        }

        [Fact]
        public async Task ThrowUserIncorrectLoginException_WhenUserIsNull_Login()
        {
            // Arrange
            A.CallTo(() => _fakeUserManager.LoginAsync(null, A<UserLocation>.Ignored)).Returns(Task.FromResult<User>(null));

            // Act
            var func = async () => await _userController.Login(null);

            // Assert
            await Assert.ThrowsAsync<UserIncorrectLoginException>(func);
        }

        [Fact]
        public async Task ExpectOkResult_WhenUserIsReturned_Login()
        {
            // Arrange
            ActionResult actual;
            var fakeUser = A.Fake<User>();

            A.CallTo(() => _fakeUserManager.LoginAsync(A<UserLogin>.Ignored, A<UserLocation>.Ignored)).Returns(Task.FromResult(fakeUser));

            // Act
            var response = await _userController.Login(null);
            actual = response.Result;

            // Assert
            Assert.IsType<OkObjectResult>(actual);
        }
    }
}

using FakeItEasy;
using IdentityApi.Controllers;
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
        public async Task ExpectNoContent_WhenUserIsNull_Login()
        {
            // Arrange
            ActionResult actual;

            A.CallTo(() => _fakeUserManager.LoginAsync(null)).Returns(Task.FromResult<User>(null));

            // Act
            var response = await _userController.Login(null);
            actual = response.Result;

            // Assert
            Assert.IsType<NoContentResult>(actual);
        }

        [Fact]
        public async Task ExpectOkResult_WhenUserIsReturned_Login()
        {
            // Arrange
            ActionResult actual;
            var fakeUser = A.Fake<User>();

            A.CallTo(() => _fakeUserManager.LoginAsync(null)).Returns(Task.FromResult(fakeUser));

            // Act
            var response = await _userController.Login(null);
            actual = response.Result;

            // Assert
            Assert.IsType<OkObjectResult>(actual);
        }
    }
}

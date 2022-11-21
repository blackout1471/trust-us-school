using IdentityApi.Models;
using System.Net;
using System.Net.Http.Json;

namespace IdentityApiIntegrationTest.User
{
    public class UserIntegrationShould : IClassFixture<IdentityApiFactory>
    {
        private readonly HttpClient _client;

        public UserIntegrationShould(IdentityApiFactory factory)
        {
            _client = factory.CreateDefaultClient();
        }

        [Fact]
        public async void ExpectStatusCode200_WhenUserLoginSuccessfully_Login()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            var newUserRequest = new UserCreate
            {
                Email = "test",
                Password = "test",
                FirstName = "test",
                LastName = "test",
                PhoneNumber = "45454545"
            };
            var customerLoginRequest = new UserLogin
            {
                Email = "test",
                Password = "test"
            };
            var createCustomerResponse = await _client.PostAsJsonAsync("api/user/Create", newUserRequest);

            // Act
            var response = await _client.PostAsJsonAsync("api/user/Login", customerLoginRequest);

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }


        [Fact]
        public async void ExpectStatusCode204_WhenUserNotExists_Login()
        {
            // Arrange
            var expected = HttpStatusCode.NoContent;
            var customerLoginRequest = new UserLogin
            {
                Email = "notexists@notexists.dk",
                Password = "notexists"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/user/Login", customerLoginRequest);

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }
    }
}
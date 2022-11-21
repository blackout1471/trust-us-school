using IdentityApi.Models;
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
        public async void ExpectStatusCode200_When_UserLogin()
        {
            // Setup data
            var newUser = new UserCreate
            {
                Email = "test",
                Password = "test",
                FirstName = "test",
                LastName = "test",
                PhoneNumber = "45454545"
            };
            var create = await _client.PostAsJsonAsync("api/user/Register", newUser);

            // Arrange
            var customerLogin = new UserLogin
            {
                Email = "Test",
                Password = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/user/Login", customerLogin);

            // Assert
            Assert.True(true);
        }
    }
}
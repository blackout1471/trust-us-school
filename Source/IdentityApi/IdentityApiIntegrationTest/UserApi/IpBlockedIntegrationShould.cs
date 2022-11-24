using DotNet.Testcontainers.Containers;
using IdentityApi.Exceptions;
using IdentityApi.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net;

namespace IdentityApiIntegrationTest.UserApi
{
    public class IpBlockedIntegrationShould : IClassFixture<IdentityApiFactory>
    {
        /// <summary>
        /// The http client used to call the api which is being tested.
        /// </summary>
        private readonly HttpClient _client;

        private readonly string _baseUrl;
        private readonly Func<string, Task<ExecResult>> _sqlQuery;

        public IpBlockedIntegrationShould(IdentityApiFactory factory)
        {
            _client = factory.HttpClient;
            _baseUrl = "api/user/";
            _sqlQuery = factory.RunSqlQuery;

            var r = _sqlQuery("Use TrustUs; while (select count(*) from dbo.UserLocation) < 30 begin Insert into dbo.UserLocation (Ip) values ('Unknown') end")
                .GetAwaiter()
                .GetResult();        
        }

        [Fact]
        public async void ExpectStatusCode403_WhenIPBlocked_Register()
        {
            // Arrange
            var expected = HttpStatusCode.Forbidden;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = TestDataHelper.GenerateNewUserRequest();

            // act
            var response = await _client.PostAsJsonAsync(_baseUrl + "create", newUserRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode403_WhenIPBlocked_Login()
        {
            // Arrange
            IpBlockedException ipBlockedException = new IpBlockedException();
            var expected = ipBlockedException.Message;
            string actual = null;

            var userLogin = new UserLogin
            {
                Email = "test@test.dk",
                Password = "Incorrect password"
            };

            // act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
            actual = await response.Content.ReadAsStringAsync();


            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);

            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }
    }
}

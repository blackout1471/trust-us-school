using DotNet.Testcontainers.Containers;
using IdentityApi.Exceptions;
using IdentityApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IdentityApiIntegrationTest.UserApi
{
    public class UserIntegrationShould : IClassFixture<IdentityApiFactory>
    {
        /// <summary>
        /// The http client used to call the api which is being tested.
        /// </summary>
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly Func<string, Task<ExecResult>> _sqlQuery;

        public UserIntegrationShould(IdentityApiFactory factory)
        {
            _client = factory.HttpClient;
            _baseUrl = "api/user/";
            _sqlQuery = factory.RunSqlQuery;
        }

        [Fact]
        public async void ExpectStatusCode200_WhenUserLoginSuccessfully_Login()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = GenerateNewUserRequest();
            var customerLoginRequest = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = newUserRequest.Password
            };
            var createCustomerResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", customerLoginRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async void ExpectStatusCode403_WhenUserNotExists_Login()
        {
            // Arrange
            var expected = HttpStatusCode.Forbidden;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var customerLoginRequest = new UserLogin
            {
                Email = "notexists@notexists.dk",
                Password = "notexists"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", customerLoginRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", null)]
        public async void ExpectStatusCode400_WhenUserMissingRequiredData_Login(string username, string password)
        {
            // Arrange
            var expected = HttpStatusCode.BadRequest;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var customerLoginRequest = new UserLogin
            {
                Email = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", customerLoginRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode200_WhenUserIsRegistered_Register()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = GenerateNewUserRequest();

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(requiredRegisterTestData))]
        public async void ExpectStatusCode400_WhenMissingRequiredData_Register(string email, string pass, string firstname, string lastname)
        {
            // Arrange
            var expected = HttpStatusCode.BadRequest;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = new UserCreate
            {
                Email = email,
                FirstName = firstname,
                LastName = lastname,
                Password = pass,
                PhoneNumber = "45454545"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode200_WhenUserExists_GetUserByToken()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = GenerateNewUserRequest();
            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            var token = await createUserResponse.Content.ReadFromJsonAsync<UserToken>();

            // Act
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "getuserbytoken"))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token?.Token);

                var response = await _client.SendAsync(requestMessage);
                actual = response.StatusCode;
            }

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode403_WhenAccountIsLockedWrongPassword_Login()
        {
            // Arrange
            AccountLockedException accountLockedException = new AccountLockedException();

            var expected = accountLockedException.Message;
            string actual = null;

            var newUserRequest = GenerateNewUserRequest();

            var userLogin = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = "Incorrect password"
            };


            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // act 
            int tries = 6;

            for (int i = 0; i < tries; i++)
            {
                var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
                actual = await response.Content.ReadAsStringAsync();
            }

            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);

            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }


        [Fact]
        public async void ExpectStatusCode403_WhenLoginFromDiffrentIP_Login()
        {
            // Arrange
            var exception = new Required2FAException();

            var expected = exception.Message; 
            string actual = null;

            var newUserRequest = GenerateNewUserRequest();

            var userLogin = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = newUserRequest.Password
            };


            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // update the ip adress of newly created user
            await _sqlQuery($"use trustus; update UserLocation set IP = 'UserIP' where UserID = (select id from Users where Email = '{newUserRequest.Email}')");

            // act 

            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
            actual = await response.Content.ReadAsStringAsync();

            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);

            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }

        [Fact]
        public async void ExpectStatusCode403_WhenLoginFromWithDiffrentBrowser_Login()
        {
            // Arrange
            var exception = new Required2FAException();
            var expected = exception.Message; 
            string actual = null;

            var newUserRequest = GenerateNewUserRequest();

            var userLogin = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = newUserRequest.Password
            };


            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // update the user agent of newly created user
            await _sqlQuery($"use trustus; update UserLocation set UserAgent = 'User FireFox Browser' where UserID = (select id from Users where Email = '{newUserRequest.Email}')");

            // act 

            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
            actual = await response.Content.ReadAsStringAsync();

            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);

            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }

        #region TestData
        public static IEnumerable<object[]> requiredRegisterTestData => new List<object[]>
        {
            new object[] { null, "pass1234567", "f", "n" },
            new object[] { "email", null, "first", "last" },
            new object[] { "email", "pass1234567", null, "last" },
            new object[] { "email", "pass1234567", "first", null },
            new object[] { "email", new String('a', 7), "first", "last" },
            new object[] { "email", new String('a', 129), "first", "last" },
        };

        /// <summary>
        /// Helper method to generate a new valid user request.
        /// </summary>
        private UserCreate GenerateNewUserRequest()
        {
            var rnd = Guid.NewGuid()
                .ToString("n")
                .Substring(0, 8);

            return new UserCreate()
            {
                Email = $"test{rnd}@test.dk",
                Password = $"test{rnd}test",
                FirstName = $"test{rnd}",
                LastName = $"test",
                PhoneNumber = "454545452"
            };
        }
        #endregion

    }
}
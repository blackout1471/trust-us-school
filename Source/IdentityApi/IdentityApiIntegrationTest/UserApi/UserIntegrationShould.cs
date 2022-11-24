using DotNet.Testcontainers.Containers;
using IdentityApi.Exceptions;
using IdentityApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

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
            var newUserRequest = TestDataHelper.GenerateNewUserRequest();
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
            var newUserRequest = TestDataHelper.GenerateNewUserRequest();

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode200_WhenUserRegistersWithEmojiPassword_Register_Login()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            HttpStatusCode actualRegister = HttpStatusCode.InternalServerError;
            HttpStatusCode actualLogin = HttpStatusCode.InternalServerError;

            var newUserRequest = TestDataHelper.GenerateNewUserRequest();
            newUserRequest.Password = "Pass123456❤️❤️❤️";

            var userLoginRequest = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = newUserRequest.Password
            };

            // Act
            var createResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actualLogin = createResponse.StatusCode;

            var loginResponse = await _client.PostAsJsonAsync(_baseUrl + "login", userLoginRequest);
            actualRegister = loginResponse.StatusCode;

            // Assert
            Assert.Equal(expected, actualRegister);
            Assert.Equal(expected, actualLogin);
        }

        [Theory]
        [MemberData(nameof(RequiredRegisterTestData))]
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
            var newUserRequest = TestDataHelper.GenerateNewUserRequest();
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
        public async Task ExpectStatusCode403_WhenPasswordIsLeaked_Register()
        {
            // Arrange
            var expected = HttpStatusCode.Forbidden;
            var actual = HttpStatusCode.InternalServerError;
            var newUserRequest = TestDataHelper.GenerateNewUserRequest();
            newUserRequest.Password = "breachedpassword";

            // setup sql data
            var stringBytes = Encoding.UTF8.GetBytes(newUserRequest.Password);
            var hashedBytes = SHA1.HashData(stringBytes);
            var hashedPassword = Convert.ToHexString(hashedBytes);

            var sql = $"use StoredPasswords; insert into dbo.LeakedPasswords values ('{hashedPassword}', {0})";
            await _sqlQuery(sql);

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actual = response.StatusCode;


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

            var newUserRequest = TestDataHelper.GenerateNewUserRequest();

            var userLogin = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = "Incorrect password"
            };


            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            var message = createUserResponse.Content.ReadAsStringAsync();
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

            var newUserRequest = TestDataHelper.GenerateNewUserRequest();

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

            var newUserRequest = TestDataHelper.GenerateNewUserRequest();

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

        #region TestData Setup
        public static IEnumerable<object[]> RequiredRegisterTestData => new List<object[]>
        {
            new object[] { null, "pass1234567", "f", "n" },
            new object[] { "email", null, "first", "last" },
            new object[] { "email", "pass1234567", null, "last" },
            new object[] { "email", "pass1234567", "first", null },
            new object[] { "email", new String('a', 7), "first", "last" },
            new object[] { "email", new String('a', 129), "first", "last" },
        };
        #endregion

    }
}
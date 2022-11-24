using DotNet.Testcontainers.Containers;
using IdentityApi.Exceptions;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc;
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
            var newUserRequest = new UserCreate
            {
                Email = "test2",
                Password = "test2",
                FirstName = "test2",
                LastName = "test2",
                PhoneNumber = "454545452"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);
            actual = response.StatusCode;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "", "", "")]
        [InlineData("", null, "", "")]
        [InlineData("", "", null, "")]
        [InlineData("", "", "", null)]
        public async void ExpectStatusCode400_WhenMissingRequiredData_Register(string email, string pass, string first, string last)
        {
            // Arrange
            var expected = HttpStatusCode.BadRequest;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = new UserCreate
            {
                Email = email,
                Password = pass,
                FirstName = first,
                LastName = last,
                PhoneNumber = "454545452"
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
            var newUserRequest = new UserCreate
            {
                Email = "test3",
                Password = "test3",
                FirstName = "test3",
                LastName = "test3",
                PhoneNumber = "454545452"
            };
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
            var newUserRequest = new UserCreate
            {
                Email = "test4",
                Password = "breachedpassword",
                FirstName = "test4",
                LastName = "test4",
                PhoneNumber = "454545452"
            };

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

            var newUserRequest = new UserCreate
            {
                Email = "test5",
                Password = "test4123123",
                FirstName = "test4",
                LastName = "test4",
                PhoneNumber = "454545452"
            };

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

            var newUserRequest = new UserCreate
            {
                Email = "test6",
                Password = "test5",
                FirstName = "test5",
                LastName = "test",
                PhoneNumber = "454545452"
            };

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

            var newUserRequest = new UserCreate
            {
                Email = "test7",
                Password = "test6",
                FirstName = "test6",
                LastName = "test",
                PhoneNumber = "454545452"
            };

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

    }
}
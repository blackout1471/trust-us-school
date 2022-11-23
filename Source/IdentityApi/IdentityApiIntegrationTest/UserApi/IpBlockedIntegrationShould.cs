using DotNet.Testcontainers.Containers;
using IdentityApi.Exceptions;
using IdentityApi.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace IdentityApiIntegrationTest.UserApi
{
    public class IpBlockedIntegrationShould : IClassFixture<IdentityApiFactory>, IAsyncLifetime
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
        }



        [Fact]
        public async void ExpectStatusCode403_WhenIPBlockedWithWrongEmail_Login()
        {
            // Arrange
            IpBlockedException ipBlockedException = new IpBlockedException();
            var expected = ipBlockedException.Message;
            string actual = null;

            // lock the ip adress


            var newUserRequest = new UserCreate
            {
                Email = "test7",
                Password = "test7",
                FirstName = "test7",
                LastName = "test7",
                PhoneNumber = "454545452"
            };

            var userLogin = new UserLogin
            {
                Email = "Very bad email",
                Password = newUserRequest.Password
            };


            await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // act 

            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
            actual = await response.Content.ReadAsStringAsync();


            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);
            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }


        [Fact]
        public async void ExpectStatusCode403_WhenIPBlocked_Register()
        {
            // Arrange
            var expected = HttpStatusCode.Forbidden;
            HttpStatusCode actual = HttpStatusCode.InternalServerError;
            var newUserRequest = new UserCreate
            {
                Email = "test5",
                Password = "test5",
                FirstName = "test5",
                LastName = "test5",
                PhoneNumber = "454545452"
            };
            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // act 


            var response = await _client.PostAsJsonAsync(_baseUrl + "create", newUserRequest);
            actual = response.StatusCode;


            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ExpectStatusCode403_WhenIPBlockedWithWrongPassword_Login()
        {
            for (int i = 0; i < 35; i++)
            {
                var r = await _sqlQuery("use trustus; insert into userlocation(IP) values ('172.17.0.1')");
            }

            //var r = await _sqlQuery("use trustus;\r\nalter procedure dbo.SP_IsIPLocked as select 1;");
            // Arrange
            IpBlockedException ipBlockedException = new IpBlockedException();
            var expected = ipBlockedException.Message;
            string actual = null;
            // lock the ip adress
            var newUserRequest = new UserCreate
            {
                Email = "test6",
                Password = "test6",
                FirstName = "test6",
                LastName = "test6",
                PhoneNumber = "454545452"
            };

            var userLogin = new UserLogin
            {
                Email = newUserRequest.Email,
                Password = "Incorrect password"
            };


            var createUserResponse = await _client.PostAsJsonAsync(_baseUrl + "Create", newUserRequest);

            // act 

            var response = await _client.PostAsJsonAsync(_baseUrl + "Login", userLogin);
            actual = await response.Content.ReadAsStringAsync();


            JToken jObject = JsonConvert.DeserializeObject<JToken>(actual);

            // Assert
            Assert.Equal(expected, jObject.Value<string>("error"));
        }

        public async Task InitializeAsync()
        {
            //var r = await _sqlQuery("alter procedure dbo.SP_IsIPLocked as select 1");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

using System.Net.Http.Json;

namespace Stress_Test_Web
{
    internal class LoginTest : LoadTest
    {        
        public object Request { get; set; }

        public LoginTest(string ip, int amount, HttpClient client) : base(ip, "api/user/login", amount, client)
        {
        }

        public override object GenerateRequest()
        {
            return Request;
        }

        public async override Task Setup()
        {
            var email = $"{Guid.NewGuid()}@asd.dk";
            var pass = "Stringst";

            var request = new
            {
                email = email,
                firstName = "string",
                lastName = "string",
                phoneNumber = "string",
                password = pass
            };

            var response = await Client.PostAsJsonAsync("http://" + Ip + $"/api/user/create", request);

            Request = new
            {
                Email = email,
                Password = pass
            };
        }
    }
}

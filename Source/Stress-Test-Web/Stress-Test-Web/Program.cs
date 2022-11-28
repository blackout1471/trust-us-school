using System.Diagnostics;
using System.Net.Http.Json;

namespace Stress_Test_Web
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();

            Console.WriteLine("Write server ip. ex: 10.108.149.14");
            var ip = Console.ReadLine();

            Console.WriteLine("Write amount:");
            var amount = int.Parse(Console.ReadLine());

            var loginTest = new LoginTest(ip, amount, client);

            var result = loginTest.RunTests().Result;

            Console.WriteLine($"{amount} request took |");
            Console.WriteLine(result);

            Console.ReadKey();
        }

        private async static Task<Credentials> CreateRandomUser(HttpClient client, string ip)
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

            var response = await client.PostAsJsonAsync("http://" + ip + $"/api/user/create", request);
            return new Credentials { Email = email, Password = pass };
        }
    }

    struct Credentials
    {
        public string Email;
        public string Password;
    }
}
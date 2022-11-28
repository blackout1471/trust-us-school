using System.Diagnostics;
using System.Net.Http.Json;

namespace Stress_Test_Web
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var amount = 500;

            HttpClient client = new HttpClient();


            Console.WriteLine("Write server ip. ex: 127.0.0.1");
            var ip = "10.108.149.14:80";

            var credentials = CreateRandomUser(client, ip).Result;
            var request = new { email = credentials.Email, password = credentials.Password };


            var loginTasks = new List<Task>();
            for (int i = 0; i < amount; i++)
            {
                loginTasks.Add(Task.Run( async () => await client.PostAsJsonAsync("http://" + ip + $"/api/user/login", request)));
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Task.WhenAll(loginTasks.ToArray()).Wait();
            watch.Stop();

            Console.WriteLine($"{amount} requests took {watch.ElapsedMilliseconds}ms total and {watch.ElapsedMilliseconds / amount}ms in average");

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
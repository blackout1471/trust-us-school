using System.Diagnostics;
using System.Net.Http.Json;

namespace Stress_Test_Web
{
    internal abstract class LoadTest
    {
        public string Ip { get; set; }

        public string Endpoint { get; set; }

        public int Amount { get; set; }

        public HttpClient Client { get; private set; }

        public LoadTest(string ip, string endpoint, int amount, HttpClient client)
        {
            Ip = ip;
            Endpoint = endpoint;
            Amount = amount;
            Client = client;
        }

        public async Task<TotalTimeResult> RunTests()
        {
            await Setup();

            var loginTasks = new List<Task<SingleResult>>();
            for (int i = 0; i < Amount; i++)
            {
                loginTasks.Add(Task.Run(async () =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var response = await Client.PostAsJsonAsync($"http://{Ip}/{Endpoint}", GenerateRequest());
                    stopwatch.Stop();

                    return new SingleResult()
                    {
                        Time = stopwatch.ElapsedMilliseconds,
                        Succeed = response.IsSuccessStatusCode
                    };
                }
                ));
            }

            var totalStopwatch = new Stopwatch();
            totalStopwatch.Start();
            await Task.WhenAll(loginTasks.ToArray());
            totalStopwatch.Stop();

            var totalSuccess = loginTasks.Where(t => t.Result.Succeed);

            return new TotalTimeResult
            {
                Total = totalStopwatch.ElapsedMilliseconds,
                Longest = totalSuccess.Max(t => t.Result.Time),
                Shortest = totalSuccess.Min(t => t.Result.Time),
                TotalSuccess = totalSuccess.Count(),
            };
        }

        public abstract object GenerateRequest();

        public virtual Task Setup() { return Task.CompletedTask; }
    }

    struct TotalTimeResult
    {
        public long Total;
        public long Longest;
        public long Shortest;
        public int TotalSuccess;

        public override string ToString()
        {
            return $"Total: {Total}ms\nLongest: {Longest}ms\nShortest: {Shortest}ms\nAmount succeeded: {TotalSuccess}";
        }
    }

    struct SingleResult
    {
        public bool Succeed;
        public long Time;
    }
}


using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using IdentityApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace IdentityApiIntegrationTest
{
    public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        /// <summary>
        /// The docker database container.
        /// </summary>
        private readonly MsSqlTestcontainer _dbContainer =
            new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Database = "TrustUS",
                Password = "Pass1234",
            })
            .Build();

        /// <summary>
        /// The client used to call the underlying api.
        /// </summary>
        public HttpClient HttpClient { get; private set; } = default!;

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>{
                        { "ConnectionStrings:SQLserver",  _dbContainer.ConnectionString},
                        { "ConnectionStrings:SQLserverLeaked", _dbContainer.ConnectionString.Replace("TrustUS", "StoredPasswords")}
                    })
                    .Build();

                config.AddConfiguration(integrationConfig);

            });
        }

        /// <inheritdoc />
        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var dbScheme = await File.ReadAllTextAsync(@"..\..\..\..\..\..\Scripts\DbScheme.sql");
            await _dbContainer.ExecScriptAsync(dbScheme);

            var leakedScheme = await File.ReadAllTextAsync(@"..\..\..\..\..\..\Scripts\DbPwnCheme.sql");
            await _dbContainer.ExecScriptAsync(leakedScheme);

            HttpClient = CreateClient();
        }

        /// <summary>
        /// Runs the given sql query in the test integration database.
        /// </summary>
        public async Task<ExecResult> RunSqlQuery(string query)
        {
            return await _dbContainer.ExecScriptAsync(query);
        }
    }
}

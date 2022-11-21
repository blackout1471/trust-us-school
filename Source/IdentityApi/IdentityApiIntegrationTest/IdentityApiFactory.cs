﻿
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using IdentityApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityApiIntegrationTest
{
    public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlTestcontainer _dbContainer =
            new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Database = "TrustUS",
                Password = "Pass1234",

            })
            .Build();

        

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _dbContainer.ExecScriptAsync(@"C:\\repos\\trust-us-school\\Scripts\\DbScheme.sql")
                .Wait();

            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>{{ "ConnectionStrings:SQLserver",  _dbContainer.ConnectionString}})
                    .Build();

                config.AddConfiguration(integrationConfig);

            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }
    }
}

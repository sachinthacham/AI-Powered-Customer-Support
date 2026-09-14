using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupportIQ.Application.Abstractions;
using SupportIQ.IntegrationTests.Fakes;
using Testcontainers.MsSql;
using Xunit;

namespace SupportIQ.IntegrationTests;

/// <summary>
/// Boots the real API host against a real, disposable SQL Server (via Testcontainers) so
/// persistence, migrations, auth, and validation are all exercised for real. The only
/// substitutions are the AI provider and vector store (see Fakes/) - integration tests must
/// never call a real OpenAI API or require a running Qdrant instance.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public FakeTicketAiService TicketAiService { get; } = new();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        // WebApplicationFactory's host-discovery mechanism invokes Program's top-level statements
        // once against the real process environment before our ConfigureAppConfiguration override
        // ever applies (it needs to intercept host *building*, which happens after our fail-fast
        // config checks run). Setting real environment variables - not just IConfiguration - is
        // what makes that first pass see a valid connection string and JWT secret.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _sqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-secret-at-least-32-chars");
        Environment.SetEnvironmentVariable("Ai__ApiKey", "");
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _sqlContainer.GetConnectionString(),
                ["Jwt:Secret"] = "integration-test-signing-secret-at-least-32-chars",
                ["Ai:ApiKey"] = "", // deliberately unset: proves AI calls only ever go through the fakes below
                // The fake hashed bag-of-words embedding (see Fakes/FakeEmbeddingService) is much
                // cruder than a real embedding model - it only captures literal word overlap, so a
                // short keyword query scores lower against a longer paragraph than real semantic
                // embeddings would. Lowered here so relevance-threshold *plumbing* is still
                // meaningfully tested without depending on real embedding quality.
                ["Rag:MinRelevanceScore"] = "0.4"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITicketAiService>();
            services.AddSingleton<ITicketAiService>(TicketAiService);

            services.RemoveAll<IEmbeddingService>();
            services.AddSingleton<IEmbeddingService, FakeEmbeddingService>();

            services.RemoveAll<IVectorStore>();
            services.AddSingleton<IVectorStore, FakeVectorStore>();

            services.RemoveAll<IRagService>();
            services.AddSingleton<IRagService, FakeRagService>();
        });
    }
}

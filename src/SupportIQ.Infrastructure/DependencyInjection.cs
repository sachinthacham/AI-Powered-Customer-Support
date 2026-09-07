using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using Qdrant.Client;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Options;
using SupportIQ.Infrastructure.AI;
using SupportIQ.Infrastructure.Configuration;
using SupportIQ.Infrastructure.Identity;
using SupportIQ.Infrastructure.Persistence;
using SupportIQ.Infrastructure.Persistence.Repositories;
using SupportIQ.Infrastructure.VectorStore;
using System.ClientModel;

namespace SupportIQ.Infrastructure;

/// <summary>
/// Composition root for everything the Infrastructure project provides. Kept as a single
/// extension method (rather than scattering AddX calls through Program.cs) so the API project
/// only ever needs to know "this layer exists", not what is inside it.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddIdentity(services, configuration);
        AddAi(services, configuration);
        AddRag(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<SupportIqDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(SupportIqDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SupportIqDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
    }

    private static void AddAi(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            var clientOptions = new OpenAIClientOptions();
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                clientOptions.Endpoint = new Uri(options.BaseUrl);

            var apiKey = string.IsNullOrWhiteSpace(options.ApiKey) ? "not-configured" : options.ApiKey;
            return new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        });

        services.AddSingleton<ITicketAiService, OpenAiTicketAiService>();
        services.AddSingleton<IEmbeddingService, OpenAiEmbeddingService>();
    }

    private static void AddRag(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.Configure<RagOptions>(configuration.GetSection(RagOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(options.Host, options.Port, options.UseHttps, options.ApiKey);
        });

        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddSingleton<IRagService, RagService>();
    }
}

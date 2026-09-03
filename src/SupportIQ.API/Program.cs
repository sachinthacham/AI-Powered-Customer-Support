using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SupportIQ.API.Extensions;
using SupportIQ.API.Middleware;
using SupportIQ.API.Services;
using SupportIQ.Application;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Options;
using SupportIQ.Infrastructure;
using SupportIQ.Infrastructure.Configuration;
using SupportIQ.Infrastructure.Identity;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/supportiq-.log", rollingInterval: RollingInterval.Day));

    // --- Fail fast on missing core configuration ---------------------------------------
    // The DB connection string and JWT signing secret are required for the app to function
    // at all; a missing OpenAI key only breaks AI-specific endpoints, so that only warns.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured. Set it via the ConnectionStrings__DefaultConnection environment variable.");

    // Bound once here so token issuance (JwtTokenService, via IOptions<JwtOptions>) and
    // token validation (below) can never disagree about issuer/audience/secret.
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
        throw new InvalidOperationException(
            "Jwt:Secret is not configured (or is too short). Set a secret of at least 32 characters via the Jwt__Secret environment variable.");

    if (string.IsNullOrWhiteSpace(builder.Configuration["Ai:ApiKey"]))
        Log.Warning("Ai:ApiKey is not configured - AI analysis and RAG endpoints will fail until it is set.");

    // --- Services ------------------------------------------------------------------------
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.Configure<AiConfidenceOptions>(builder.Configuration.GetSection(AiConfidenceOptions.SectionName));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sql-server", tags: new[] { "ready" })
        .AddQdrant(
            sp => sp.GetRequiredService<Qdrant.Client.QdrantClient>(),
            name: "qdrant",
            tags: new[] { "ready" });

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SupportIQ API",
            Version = "v1",
            Description = "AI-powered customer support ticketing with RAG-grounded policy answers. " +
                           "Create tickets, run AI analysis (category/priority/sentiment/summary/tags/response), " +
                           "and ask policy questions answered from the ingested knowledge base with cited sources.",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a JWT access token obtained from POST /api/auth/login."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

        options.EnableAnnotations();
    });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.Configure<JsonOptions>(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    // Swagger is exposed in every environment - for this portfolio project, Swagger UI *is*
    // the primary client, not just a development aid.
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SupportIQ API v1");
        options.DocumentTitle = "SupportIQ API";
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthCheckJsonWriter.WriteAsync });

    // Applying migrations and seeding at startup is a pragmatic choice for a demo/portfolio
    // app so `docker compose up` "just works". A real production deployment would run
    // migrations as an explicit CI/CD step instead of coupling them to app startup.
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportIQ.Infrastructure.Persistence.SupportIqDbContext>();
        await db.Database.MigrateAsync();

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await SupportIQ.Infrastructure.Persistence.Seed.DbSeeder.SeedAsync(db, passwordHasher);
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SupportIQ API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Marker class exposing <c>Program</c> to WebApplicationFactory in integration tests.</summary>
public partial class Program
{
}

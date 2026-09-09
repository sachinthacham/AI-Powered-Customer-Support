using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupportIQ.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add` construct the DbContext without spinning up the whole API
/// host (and without needing a real database connection - schema generation doesn't connect).
/// Never used at application runtime; Program.cs wires the real, environment-configured
/// connection string through <see cref="DependencyInjection.AddInfrastructure"/> instead.
/// </summary>
public class SupportIqDbContextFactory : IDesignTimeDbContextFactory<SupportIqDbContext>
{
    public SupportIqDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost,1433;Database=SupportIQ;User Id=sa;Password=Design_Time_Only;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<SupportIqDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SupportIqDbContext(optionsBuilder.Options);
    }
}

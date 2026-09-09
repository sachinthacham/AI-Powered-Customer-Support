using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SupportIQ.Infrastructure.Persistence;

/// <summary>
/// SQL Server's datetime2 has no timezone concept, so EF Core hands back <see cref="DateTime"/>
/// values with <see cref="DateTimeKind.Unspecified"/> after a round trip. Every timestamp in this
/// system is UTC by convention (see domain entity factories), so this converter re-stamps values
/// as <see cref="DateTimeKind.Utc"/> on read and normalizes to UTC on write, applied globally in
/// <see cref="SupportIqDbContext.ConfigureConventions"/>.
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

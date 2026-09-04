using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Abstractions;

public record AuthToken(string Value, DateTime ExpiresAtUtc);

/// <summary>Issues signed JWT access tokens for authenticated agents.</summary>
public interface ITokenService
{
    AuthToken GenerateToken(SupportAgent agent);
}

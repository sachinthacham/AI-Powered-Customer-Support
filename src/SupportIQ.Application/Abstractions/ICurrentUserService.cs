namespace SupportIQ.Application.Abstractions;

/// <summary>Exposes the authenticated agent for the current request, read from the JWT claims in the API layer.</summary>
public interface ICurrentUserService
{
    Guid? AgentId { get; }

    string? Email { get; }
}

using SupportIQ.Domain.Enums;

namespace SupportIQ.Domain.Entities;

/// <summary>
/// A human support agent who can authenticate and be assigned tickets.
/// Doubles as the identity record backing JWT authentication.
/// </summary>
public class SupportAgent
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public AgentRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SupportAgent()
    {
    }

    public static SupportAgent Create(string name, string email, string passwordHash, AgentRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new SupportAgent
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }
}

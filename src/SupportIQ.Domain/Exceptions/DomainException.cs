namespace SupportIQ.Domain.Exceptions;

/// <summary>
/// Base type for exceptions that represent a violation of a domain invariant
/// (as opposed to infrastructure, validation, or "not found" concerns).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

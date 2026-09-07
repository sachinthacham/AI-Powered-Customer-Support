namespace SupportIQ.Domain.Exceptions;

/// <summary>
/// Thrown when an operation is attempted against a <see cref="Entities.SupportTicket"/>
/// that is not valid for its current <see cref="Enums.TicketStatus"/>.
/// </summary>
public class InvalidTicketStateException : DomainException
{
    public InvalidTicketStateException(string message) : base(message)
    {
    }
}

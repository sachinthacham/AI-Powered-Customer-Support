namespace SupportIQ.Application.Common.Exceptions;

/// <summary>Thrown when a requested entity does not exist. Maps to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}

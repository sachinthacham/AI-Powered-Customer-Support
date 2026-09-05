namespace SupportIQ.Application.Common.Exceptions;

/// <summary>
/// Thrown when a non-AI external dependency (e.g. the Qdrant vector store) is unreachable
/// or returns an error. Maps to HTTP 503.
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

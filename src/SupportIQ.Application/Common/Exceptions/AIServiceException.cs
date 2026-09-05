namespace SupportIQ.Application.Common.Exceptions;

/// <summary>
/// Thrown when an AI provider call fails, times out, or returns output that cannot be
/// trusted (e.g. fails structured-output validation). Maps to HTTP 502/503 - the caller's
/// request was fine, but the AI dependency could not fulfil it.
/// </summary>
public class AIServiceException : Exception
{
    public AIServiceException(string message) : base(message)
    {
    }

    public AIServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

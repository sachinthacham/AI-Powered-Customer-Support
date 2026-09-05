using FluentValidation.Results;

namespace SupportIQ.Application.Common.Exceptions;

/// <summary>
/// Thrown by the MediatR validation pipeline behavior when one or more FluentValidation
/// validators fail. Maps to HTTP 400/422 via <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

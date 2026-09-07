using FluentValidation;

namespace SupportIQ.Application.Features.Tickets.Queries.SearchTickets;

public class SearchTicketsQueryValidator : AbstractValidator<SearchTicketsQuery>
{
    public SearchTicketsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetUserBooksQuery(string UserName, string Status, int Offset, int Limit) : IQuery<BooksPageResponse>;

public sealed class GetUserBooksQueryValidator : AbstractValidator<GetUserBooksQuery>
{
    public GetUserBooksQueryValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Status).Must(x => x is "read" or "reading");
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}

public sealed class GetUserBooksQueryHandler(IShelfRepository shelf) : IQueryHandler<GetUserBooksQuery, BooksPageResponse>
{
    public async Task<Result<BooksPageResponse>> Handle(GetUserBooksQuery request, CancellationToken ct)
    {
        if (!ShelfMapping.TryStatus(request.Status, out var status)) return Result.Failure<BooksPageResponse>(DomainErrors.Shelf.InvalidStatus);
        int offset = Math.Max(0, request.Offset);
        int limit = Math.Clamp(request.Limit, 1, 100);
        var page = await shelf.GetUserBooksAsync(request.UserName, status, offset, limit, ct);
        return new BooksPageResponse(page.Items.Select(x => ShelfMapping.ToResponse(x.Book, x.Status)).ToList(), page.Total, offset, limit,
            await shelf.GetLastRatingSyncAsync(ct));
    }
}

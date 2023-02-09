using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Authors.GetAuthorById;

internal sealed class GetAuthorByIdQueryHandler : IQueryHandler<GetAuthorByIdQuery, AuthorResponse>
{
    private readonly IAuthorRepository _authorRepository;

    public GetAuthorByIdQueryHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<Result<AuthorResponse>> Handle(
        GetAuthorByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(
            request.AuthorId, 
            cancellationToken);

        if (author is null)
        {
            return Result.Failure<AuthorResponse>(new Error(
                "Author.NotFound",
                $"The author with Id {request.AuthorId} was not found."));
        }

        var response = new AuthorResponse(author.Id, author.Email.Value);

        return response;
    }
}
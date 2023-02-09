using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Authors.GetAuthorById;

public record GetAuthorByIdQuery(Guid AuthorId) : IQuery<AuthorResponse>
{
}
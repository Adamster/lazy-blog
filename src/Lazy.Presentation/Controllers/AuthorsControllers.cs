using Lazy.Application.Authors.GetAuthorById;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;

[Route("/api/authors")]
public sealed class AuthorsControllers : ApiController
{
    public AuthorsControllers(ISender sender) : base(sender)
    {
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuthorById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAuthorByIdQuery(id);
        Result<AuthorResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}
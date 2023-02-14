using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;

[Route("api/posts")]
public class PostsController : ApiController
{
    public PostsController(ISender sender) : base(sender)
    {
    }
}
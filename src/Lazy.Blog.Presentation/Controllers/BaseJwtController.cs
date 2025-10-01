using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

public class BaseJwtController(ISender sender, ILogger<ApiController> logger) : ApiController(sender, logger)
{
    
}
using Lazy.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Abstractions;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected readonly ISender Sender;
    private readonly ILogger<ApiController> _logger;

    protected ApiController(ISender sender, ILogger<ApiController> logger)
    {
        Sender = sender;
        _logger = logger;
    }

    protected IActionResult HandleFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                BadRequest(
                    CreateProblemDetails(
                        "Validation Error", StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors
                    )),
            _ => BadRequest(
                CreateProblemDetails(
                "Bad Request",
                StatusCodes.Status400BadRequest,
                result.Error))
        };

    private  ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null)
    {

        ProblemDetails problemDetails =  new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Message,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };

        _logger.LogError(problemDetails.Detail);
        return problemDetails;
    }
}
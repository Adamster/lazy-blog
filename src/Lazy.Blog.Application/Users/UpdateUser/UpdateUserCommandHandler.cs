using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.UpdateUser;

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var userNameResult = UserName.Create(request.Username);
        var firstNameResult = FirstName.Create(request.FirstName);
        var lastNameResult = LastName.Create(request.LastName);
        Result<Biography>? biographyResult = null;

        if (request.Biography is not null)
        {
            biographyResult = Biography.Create(request.Biography);
        }

        var user = await userRepository.GetByIdAsync(request.Id, ct);

        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.Id));
        }

        var userNameChanged = user.UserName != request.Username;
        if (userNameChanged && await userRepository.GetByUsernameAsync(userNameResult.Value, ct) is not null)
        {
            logger.LogError("Username {0} already taken", request.Username);
            return Result.Failure(DomainErrors.UserName.UserNameAlreadyInUse);
        }

        logger.LogInformation($"Updating user {user.UserName} information");

        user.UpdateUser(
            firstNameResult.Value,
            lastNameResult.Value,
            userNameResult.Value,
            biographyResult?.Value);

        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
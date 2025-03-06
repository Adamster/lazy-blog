using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.UpdateUser;

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserCommandHandler> logger,
    UserManager<User> userManager)
    : ICommandHandler<UpdateUserCommand>
{
    private readonly ILogger<UpdateUserCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var userNameResult = UserName.Create(request.Username);
        var firstNameResult = FirstName.Create(request.FirstName);
        var lastNameResult = LastName.Create(request.LastName);
        
        var user = await userRepository.GetByIdAsync(request.Id, ct);

        if (user == null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.Id));
        }

        if (await userRepository.GetByUsernameAsync(userNameResult.Value, ct) is not null)
        {
            return Result.Failure(DomainErrors.UserName.UserNameAlreadyInUse);
        }
        
        _logger.LogInformation($"Updating user {user.UserName} information");
        user.ChangeName(
            firstNameResult.Value,
            lastNameResult.Value,
            userNameResult.Value);

        await userManager.UpdateAsync(user);

        await unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}
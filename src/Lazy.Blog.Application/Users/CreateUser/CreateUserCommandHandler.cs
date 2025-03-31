using Lazy.Application.Abstractions.Email;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    UserManager<User> userManager,
    ILogger<CreateUserCommandHandler> logger,
    IEmailService emailService)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        Result<Email> emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<Guid>(emailResult.Error);
        }

        Result<FirstName> firstNameResult = FirstName.Create(request.FirstName);
        Result<LastName> lastNameResult = LastName.Create(request.LastName);
        Result<UserName> userNameResult = UserName.Create(request.UserName);
        Result<Biography>? biographyResult = null;


        if (!string.IsNullOrEmpty(request.Biography))
        {
            biographyResult = Biography.Create(request.Biography);
        }

        if (!await userRepository.IsEmailUniqueAsync(emailResult.Value, ct))
        {
            return Result.Failure<Guid>(DomainErrors.User.EmailAlreadyInUse);
        }

        if (await userRepository.GetByUsernameAsync(userNameResult.Value, ct) is not null)
        {
            return Result.Failure<Guid>(DomainErrors.UserName.UserNameAlreadyInUse);
        }

        var user = User.Create(
            Guid.NewGuid(),
            emailResult.Value,
            firstNameResult.Value,
            lastNameResult.Value,
            userNameResult.Value,
            biographyResult?.Value);


        IdentityResult newUser = await userManager.CreateAsync(user, request.Password);
        if (!newUser.Succeeded)
        {
            var error = newUser.Errors.First();
            return Result.Failure<Guid>(new Error(error.Code, error.Description));
        }

        logger.LogInformation($"New user with email {user.Email} registered");
        
        await unitOfWork.SaveChangesAsync(ct);
        await emailService.SendWelcomeEmail(emailResult.Value.Value, userNameResult.Value.Value);
        return user.Id;
    }

}
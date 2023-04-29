using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.CreateUser;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CreateUserCommandHandler> _logger;


    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<CreateUserResponse>(emailResult.Error);
        }

        Result<FirstName> firstNameResult = FirstName.Create(request.FirstName);
        Result<LastName> lastNameResult = LastName.Create(request.LastName);
        Result<UserName> userNameResult = UserName.Create(request.UserName);

        if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
        {
            return Result.Failure<CreateUserResponse>(DomainErrors.User.EmailAlreadyInUse);
        }

        if (await _userRepository.GetByUsernameAsync(userNameResult.Value, cancellationToken) is not null)
        {
            return Result.Failure<CreateUserResponse>(DomainErrors.UserName.UserNameAlreadyInUse);
        }

        var user = User.Create(
            Guid.NewGuid(),
            emailResult.Value,
            firstNameResult.Value,
            lastNameResult.Value,
            userNameResult.Value);


        IdentityResult newUser = await _userManager.CreateAsync(user, request.Password);
        if (!newUser.Succeeded)
        {
            var error = newUser.Errors.First();
            return Result.Failure<CreateUserResponse>(new Error(error.Code, error.Description));
        }

        _logger.LogInformation($"New user with email {user.Email} registered");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateUserResponse(user.Id, user.CreatedOnUtc);
    }

}
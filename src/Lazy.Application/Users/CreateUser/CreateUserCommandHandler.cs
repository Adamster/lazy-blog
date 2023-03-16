using System.Security.Cryptography;
using System.Text;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Application.Users.CreateUser;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;


    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<Guid>(emailResult.Error);
        }

        Result<FirstName> firstNameResult = FirstName.Create(request.FirstName);
        Result<LastName> lastNameResult = LastName.Create(request.LastName);

        if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
        {
            return Result.Failure<Guid>(DomainErrors.User.EmailAlreadyInUse);
        }

        var user = User.Create(
            Guid.NewGuid(),
            emailResult.Value,
            firstNameResult.Value,
            lastNameResult.Value);

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }

}
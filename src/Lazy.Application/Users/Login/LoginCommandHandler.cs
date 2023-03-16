using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Application.Users.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(
        LoginCommand request, 
        CancellationToken cancellationToken)
    {
        Result<Email> email = Email.Create(request.Email);

        User? user = await _userRepository.GetByEmailAsync(
            email.Value, 
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(DomainErrors.User.InvalidCredentials);
        }

        var signInResult = _passwordHasher
            .VerifyHashedPassword(user, user.PasswordHash!, request.Password);

        if (signInResult == PasswordVerificationResult.Failed)
        {
            return Result.Failure<string>(DomainErrors.User.InvalidCredentials);
        }
        
        string token = _jwtProvider.Generate(user);

        return token;
    }
}
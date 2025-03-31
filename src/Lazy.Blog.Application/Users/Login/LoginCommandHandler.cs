using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.RefreshToken;
using Lazy.Domain.Entities;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Application.Users.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;


    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, 
        CancellationToken ct)
    {
        Result<Email> email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result.Failure<LoginResponse>(DomainErrors.Email.InvalidFormat);
        }
        User? user = await _userRepository.GetByEmailAsync(
            email.Value, 
            ct);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(DomainErrors.User.InvalidCredentials);
        }

        SignInResult? signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (!signInResult.Succeeded)
        {
            return Result.Failure<LoginResponse>(DomainErrors.User.InvalidCredentials);
        }

        //_roleManager.
        //  var roleResult =  await _userManager.AddToRoleAsync(user, Role.AdminRoleName);
        TokenResponse tokenResponse = await _jwtProvider.GenerateAsync(user, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            new UserResponse(user));
    }
}
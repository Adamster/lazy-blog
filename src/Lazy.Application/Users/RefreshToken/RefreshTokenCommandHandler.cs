using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Repositories.Identity;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public RefreshTokenCommandHandler(
        IJwtProvider jwtProvider, 
        IUserTokenRepository userTokenRepository,
        IUnitOfWork unitOfWork, 
        IUserRepository userRepository)
    {
        _jwtProvider = jwtProvider;
        _userTokenRepository = userTokenRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var validatedToken = _jwtProvider.GetPrincipalFromToken(request.AccessToken);

        if (validatedToken == null)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.InvalidToken);
        }

        if (!_jwtProvider.IsTokenExpired(validatedToken))
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.AccessTokenNotExpired);
        }

        UserToken? storedRefreshToken = await _userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        string accessTokenId = _jwtProvider.GetAccessTokenId(validatedToken);
        if (storedRefreshToken is null)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.NotFound);
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.ExpiredRefreshToken);
        }

        if (storedRefreshToken.IsInvalidated)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.Invalidated);
        }

        if (storedRefreshToken.IsUsed)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.IsUsed);
        }

        if (storedRefreshToken.JwtId != accessTokenId)
        {
            return Result.Failure<LoginResponse>(DomainErrors.UserToken.NotMatched);
        }

        storedRefreshToken.UseToken();
        _userTokenRepository.Update(storedRefreshToken);


        Guid userId = _jwtProvider.GetUserIdFromToken(validatedToken);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<LoginResponse>(DomainErrors.User.NotFound(userId));
        }

        TokenResponse tokenResponse = await _jwtProvider.GenerateAsync(user, cancellationToken);

        var response = new LoginResponse(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            new UserResponse(user));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
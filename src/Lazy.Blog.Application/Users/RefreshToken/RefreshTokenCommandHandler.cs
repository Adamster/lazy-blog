﻿using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Repositories.Identity;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
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

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        UserToken? storedRefreshToken = await _userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (storedRefreshToken is null)
        {
            return Result.Failure<RefreshTokenResponse>(DomainErrors.UserToken.NotFound);
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return Result.Failure<RefreshTokenResponse>(DomainErrors.UserToken.ExpiredRefreshToken);
        }

        if (storedRefreshToken.IsInvalidated)
        {
            return Result.Failure<RefreshTokenResponse>(DomainErrors.UserToken.Invalidated);
        }

        if (storedRefreshToken.IsUsed)
        {
            return Result.Failure<RefreshTokenResponse>(DomainErrors.UserToken.IsUsed);
        }

        storedRefreshToken.UseToken();
        _userTokenRepository.Update(storedRefreshToken);


        Guid userId = storedRefreshToken.UserId;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(DomainErrors.User.NotFound(userId));
        }

        TokenResponse tokenResponse = await _jwtProvider.GenerateAsync(user, cancellationToken);

        var response = new RefreshTokenResponse(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken);
        

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
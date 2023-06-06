﻿using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.UploadUserAvatar;

public class UploadUserAvatarCommandHandler : ICommandHandler<UploadUserAvatarCommand>
{
    private readonly IFileService _fileService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadUserAvatarCommandHandler(
        IFileService fileService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _fileService = fileService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UploadUserAvatarCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        //Add scoped user
        var uploadedUrl = await _fileService.UploadAsync(request.File, user.UserName.Value, ct);

        if (uploadedUrl is null)
        {
            return Result.Failure(DomainErrors.Avatar.UploadFailed);
        }

        var newAvatarResult = Avatar.Create(request.File.FileName, uploadedUrl);

        user.SetAvatar(newAvatarResult.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
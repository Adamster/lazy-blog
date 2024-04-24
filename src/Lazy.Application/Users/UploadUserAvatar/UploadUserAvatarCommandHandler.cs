using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
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
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UploadUserAvatarCommandHandler(
        IFileService fileService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext)
    {
        _fileService = fileService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result> Handle(UploadUserAvatarCommand request, CancellationToken ct)
    {

        if (!_currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure(DomainErrors.User.UnauthorizedUserUpdate);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        var avatarCheck = Avatar.Create(request.File.FileName, string.Empty, request.File.Length, true);
        if (avatarCheck.IsFailure)
        {
            return Result.Failure(avatarCheck.Error);
        }

        //Add scoped user
        var uploadedUrl = await _fileService.UploadAsync(request.File, user.UserName.Value, ct);

        if (uploadedUrl is null)
        {
            return Result.Failure(DomainErrors.Avatar.UploadFailed);
        }

        var newAvatarResult = Avatar.Create(request.File.FileName, uploadedUrl, request.File.Length);
        if (newAvatarResult.IsFailure)
        {
            await _fileService.DeleteAsync(request.File.FileName, ct);
            return Result.Failure(newAvatarResult.Error);
        }
        user.SetAvatar(newAvatarResult.Value);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
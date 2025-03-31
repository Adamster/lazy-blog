using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.UploadUserAvatar;

public class UploadUserAvatarCommandHandler(
    IFileService fileService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext)
    : ICommandHandler<UploadUserAvatarCommand>
{
    public async Task<Result> Handle(UploadUserAvatarCommand request, CancellationToken ct)
    {

        if (!currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure(DomainErrors.User.UnauthorizedUserUpdate);
        }

        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        var avatarCheck = Avatar.Create(request.File.FileName, string.Empty, request.File.Length, true);
        if (avatarCheck.IsFailure)
        {
            return Result.Failure(avatarCheck.Error);
        }

        var avatarMediaId = Guid.NewGuid();

        var uploadedUrl = await fileService.UploadAsync(request.File, avatarMediaId, user.UserName!, ct);

        if (uploadedUrl is null)
        {
            return Result.Failure(DomainErrors.Avatar.UploadFailed);
        }
        
        var newAvatarResult = Avatar.Create(request.File.FileName, uploadedUrl, request.File.Length);
       
        user.SetAvatar(newAvatarResult.Value);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.DeleteUserAvatar;

public class DeleteAvatarCommandHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IFileService fileService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAvatarCommand>
{
    public async Task<Result> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetCurrentUserId();

        if (currentUserId != request.UserId)
        {
            return Result.Failure(DomainErrors.User.UnauthorizedUserUpdate);
        }
        
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        
        var avatarToDelete = currentUser!.Avatar;

        if (avatarToDelete is null)
        {
            return Result.Failure(DomainErrors.Avatar.AvatarAlreadyEmpty);
        }

        var isDeleted = await fileService.DeleteByBlobUrl(avatarToDelete.Url, currentUser!.UserName!, cancellationToken);

        if (!isDeleted)
        {
            return Result.Failure(DomainErrors.Avatar.DeleteError);
        }
        
        currentUser.DeleteAvatar();

        userRepository.Update(currentUser);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Media.DeleteMedia;

public class DeleteMediaHandler(
    IFileService fileService,
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IMediaItemRepository mediaItemRepository)
    : ICommandHandler<DeleteMediaCommand>
{
    public async Task<Result> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetCurrentUserId();
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        
        MediaItem? mediaItemToDelete = await mediaItemRepository.GetByUrlAsync(request.BlobUrl, cancellationToken);

        if (mediaItemToDelete is null)
        {
            return Result.Failure(DomainErrors.MediaItem.NotFound);
        }

        if (mediaItemToDelete.UserId != currentUser!.Id)
        {
            return Result.Failure(DomainErrors.MediaItem.Unauthorized);
        }
        
        bool isDeleted = await fileService.DeleteByBlobUrl(request.BlobUrl, currentUser.UserName!,  cancellationToken);

        if (!isDeleted)
        {
            return Result.Failure(DomainErrors.MediaItem.DeleteFailed);
        }

        return Result.Success();
    }
}
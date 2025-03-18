using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Media.DeleteMedia;

public class DeleteMediaHandler  : ICommandHandler<DeleteMediaCommand>
{
    private readonly IFileService _fileService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;
    private readonly IMediaItemRepository _mediaItemRepository;

    public DeleteMediaHandler(IFileService  fileService, ICurrentUserContext currentUserContext, IUserRepository userRepository, IMediaItemRepository mediaItemRepository)
    {
        _fileService = fileService;
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
        _mediaItemRepository = mediaItemRepository;
    }
    
    public async Task<Result> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        
        
        MediaItem? mediaItemToDelete = await _mediaItemRepository.GetByUrlAsync(request.BlobUrl, cancellationToken);

        if (mediaItemToDelete is null)
        {
            return Result.Failure(DomainErrors.MediaItem.NotFound);
        }

        if (mediaItemToDelete.UserId != currentUser!.Id)
        {
            return Result.Failure(DomainErrors.MediaItem.Unauthorized);
        }
        
        bool isDeleted = await _fileService.DeleteByBlobUrl(request.BlobUrl, currentUser!.UserName!,  cancellationToken);

        if (!isDeleted)
        {
            return Result.Failure(DomainErrors.MediaItem.DeleteFailed);
        }

        return Result.Success();
    }
}
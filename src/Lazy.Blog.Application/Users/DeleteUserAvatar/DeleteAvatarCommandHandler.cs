using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.DeleteUserAvatar;

public class DeleteAvatarCommandHandler : ICommandHandler<DeleteAvatarCommand>
{
    private readonly ICurrentUserContext  _currentUserContext;
    private readonly IUserRepository  _userRepository;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteAvatarCommandHandler(ICurrentUserContext currentUserContext, IUserRepository userRepository, IFileService fileService, IUnitOfWork unitOfWork)
    {
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();

        if (currentUserId != request.UserId)
        {
            return Result.Failure(DomainErrors.User.UnauthorizedUserUpdate);
        }
        
        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        
        var avatarToDelete = currentUser!.Avatar;

        if (avatarToDelete is null)
        {
            return Result.Failure(DomainErrors.Avatar.AvatarAlreadyEmpty);
        }

        var isDeleted = await _fileService.DeleteByUrlAsync(avatarToDelete.Url, cancellationToken);

        if (!isDeleted)
        {
            return Result.Failure(DomainErrors.Avatar.DeleteError);
        }
        
        currentUser.DeleteAvatar();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
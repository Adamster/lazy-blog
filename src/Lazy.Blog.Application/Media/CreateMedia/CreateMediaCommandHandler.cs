using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Media.CreateMedia;

public class CreateMediaCommandHandler(
    IFileService fileService,
    IUserRepository userRepository,
    IMediaItemRepository mediaItemRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMediaCommand, string>
{
    public async Task<Result<string>> Handle(CreateMediaCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(DomainErrors.User.NotFound(request.UserId));
        }

        var mediaItemCandidate = Avatar.Create(request.File.FileName, string.Empty, request.File.Length, true);

        if (mediaItemCandidate.IsFailure)
        {
            return Result.Failure<string>(mediaItemCandidate.Error);
        }

        var mediaId = Guid.NewGuid();

        var uploadedUrl = await fileService.UploadAsync(request.File, mediaId, user.UserName!, cancellationToken);

        if (uploadedUrl is null)
        {
            return Result.Failure<string>(DomainErrors.Avatar.UploadFailed);
        }

        var mediaItem = MediaItem.Create(mediaId, user, uploadedUrl);

        await mediaItemRepository.Add(mediaItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mediaItem.UploadedUrl;

    }
}
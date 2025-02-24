using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Media.CreateMedia;

public class CreateMediaCommandHandler : ICommandHandler<CreateMediaCommand, string>
{
    private readonly IFileService _fileService;
    private readonly IUserRepository _userRepository;
    private readonly IMediaItemRepository _mediaItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMediaCommandHandler(
        IFileService fileService,
        IUserRepository userRepository,
        IMediaItemRepository mediaItemRepository,
        IUnitOfWork unitOfWork)
    {
        _fileService = fileService;
        _userRepository = userRepository;
        _mediaItemRepository = mediaItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(CreateMediaCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<string>(DomainErrors.User.NotFound(request.UserId));
        }

        var mediaItemCandidate = Avatar.Create(request.File.FileName, string.Empty, request.File.Length, true);

        if (mediaItemCandidate.IsFailure)
        {
            return Result.Failure<string>(mediaItemCandidate.Error);
        }

        var uploadedUrl = await _fileService.UploadAsync(request.File, user.UserName.Value, cancellationToken);

        if (uploadedUrl is null)
        {
            return Result.Failure<string>(DomainErrors.Avatar.UploadFailed);
        }

        var mediaItemToUpload = Avatar.Create(
            request.File.FileName,
            uploadedUrl,
            request.File.Length);

        if (mediaItemToUpload.IsFailure)
        {
            await _fileService.DeleteAsync(request.File.FileName, cancellationToken);
            return Result.Failure<string>(mediaItemToUpload.Error);
        }

        var mediaItem = MediaItem.Create(user, uploadedUrl);

        await _mediaItemRepository.Add(mediaItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return mediaItem.UploadedUrl;

    }
}
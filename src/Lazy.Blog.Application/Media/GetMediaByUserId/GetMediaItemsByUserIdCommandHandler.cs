using System.Web;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Media.GetMediaByUserId;

public class GetMediaItemsByUserIdCommandHandler(
    IMediaItemRepository mediaItemRepository,
    ICurrentUserContext currentUserContext)
    : IQueryHandler<GetMediaItemsByUserIdQuery, List<MediaItemResponse>>
{
    public async Task<Result<List<MediaItemResponse>>> Handle(GetMediaItemsByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure<List<MediaItemResponse>>(DomainErrors.MediaItem.Unauthorized);
        }
        
        List<MediaItem> mediaItems = await mediaItemRepository.GetByUserId(request.UserId, cancellationToken);

        var result = mediaItems
            .Select(x => new MediaItemResponse(x.Id, x.UploadedUrl, ExtractFileName(x.UploadedUrl)))
            .ToList();

        return result;
    }
    
    private static string ExtractFileName(string url)
    {
        var uri = new Uri(url);
        var encodedFileName = uri.Segments[^1];
        var decodedFileName = HttpUtility.UrlDecode(encodedFileName);
        return decodedFileName;
    }
}
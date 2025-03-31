using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Media.GetMediaByUserId;

public record GetMediaItemsByUserIdQuery(Guid UserId) : IQuery<List<MediaItemResponse>>;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Presentation.Contracts.Media;

namespace Lazy.Application.Media.GetMediaByUserId;

public record GetMediaItemsByUserIdQuery(Guid UserId) : IQuery<List<MediaItemResponse>>;
using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.GetPostRatingHistory;

public record GetPostRatingHistoryQuery(string Slug) : IQuery<PostRatingHistoryResponse>;

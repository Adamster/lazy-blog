using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.GetPostByUserId;

public record GetPostByUserIdQuery(Guid UserId, int Offset) : IQuery<List<UserPostResponse>>;
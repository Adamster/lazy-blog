using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPostByUserId;

namespace Lazy.Application.Posts.GetPostByUserName;

public record GetPostByUserNameQuery(string UserName, int Offset) : IQuery<List<UserPostResponse>>;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPostByUserId;

public record UserPostResponse(UserResponse User, List<UserPostItem> PostItems, int TotalPostCount);
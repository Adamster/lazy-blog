using Lazy.Application.Comments.GetCommentById;
using Lazy.Domain.Entities;

namespace Lazy.Application.Users.Extensions;

public static class UserMapper
{
    public static UserCommentResponse ToUserCommentResponse(this User user) =>
        new(
            user.Id,
            user.DisplayName.Value,
            user.UserName!,
            user.Avatar?.Url
        );
}

using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;

namespace Lazy.Application.Posts.Extensions;

public static class PostMapper
{
    public static List<DisplayPostResponse> ToListDisplayPostResponse(this IEnumerable<Post> posts)
    {
        var result = posts
            .Select(p =>
                new DisplayPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary!.Value,
                    p.Slug.Value,
                    p.IsPublished,
                    new UserResponse(p.User),
                    p.Views,
                    p.Comments.Count,
                    p.Rating,
                    p.User.PostVotes
                        .Where(v => v.PostId == p.Id)
                        .Select(v => v.VoteDirection)
                        .FirstOrDefault(),
                    p.CoverUrl,
                    p.Tags.Select(x => new TagResponse(x.Id, x.Value)).ToList(),
                    p.CreatedOnUtc)).ToList();

        return result;
    }

    public static List<UserPostItem> ToUserPostItemResponse(this IEnumerable<Post> posts)
    {
        var result = posts.Select(p =>
                new UserPostItem(
                    p.Id,
                    p.Title.Value,
                    p.Summary!.Value,
                    p.Slug.Value,
                    p.Views,
                    p.Comments.Count,
                    p.Rating,
                    p.User.PostVotes
                        .Where(v => v.PostId == p.Id)
                        .Select(v => v.VoteDirection)
                        .FirstOrDefault(),
                    p.CoverUrl,
                    p.IsPublished,
                    p.CreatedOnUtc))
            .ToList();
        
        return result;
    }
}
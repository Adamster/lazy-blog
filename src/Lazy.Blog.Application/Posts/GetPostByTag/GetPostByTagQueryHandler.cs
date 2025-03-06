using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Extensions;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByTag;

public class GetPostByTagQueryHandler(
    IPostRepository postRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetPostByTagQuery, List<DisplayPostResponse>>
{
    public Task<Result<List<DisplayPostResponse>>> Handle(GetPostByTagQuery request, CancellationToken ct)
    {
        Tag? tag = tagRepository.GetTagByValue(request.Tag);
        if (tag is null)
        {
            return Task.FromResult(Result.Failure<List<DisplayPostResponse>>(DomainErrors.Tag.NotFound(request.Tag)));
        }

        var posts = postRepository.GetPostsByTag(tag, ct);
        var response = posts.ToListDisplayPostResponse();
        return Task.FromResult<Result<List<DisplayPostResponse>>>(response);
    }
}
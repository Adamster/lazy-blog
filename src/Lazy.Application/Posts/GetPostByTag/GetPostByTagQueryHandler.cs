using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByTag;

public class GetPostByTagQueryHandler : IQueryHandler<GetPostByTagQuery,List<PublishedPostResponse>>
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public GetPostByTagQueryHandler(IPostRepository postRepository,
        ITagRepository tagRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
    }


    public async Task<Result<List<PublishedPostResponse>>> Handle(GetPostByTagQuery request, CancellationToken cancellationToken)
    {
        Tag? tag = _tagRepository.GetTagByValue(request.Tag);
        if (tag is null)
        {
            return Result.Failure<List<PublishedPostResponse>>(DomainErrors.Tag.NotFound(request.Tag));
        }

        IList<Post> posts = await _postRepository.GetPostsByTagAsync(tag, cancellationToken);

        var response = posts
            .Select(p =>
                new PublishedPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary?.Value,
                    p.Slug.Value,
                    new UserResponse(p.UserId, p.User.Email.Value, p.User.FirstName.Value, p.User.LastName.Value,
                        p.User.UserName.Value, p.User.CreatedOnUtc),
                    p.Views,
                    p.Comments.Count,
                    p.Rating,
                    p.User.PostVotes.FirstOrDefault(u => u.PostId == p.Id)?.VoteDirection,
                    p.CoverUrl,
                    p.Tags.Select(x => new TagResponse(x.Id, x.Value)).ToList(),
                    p.CreatedOnUtc))
            .ToList();

        return response;
    }
}
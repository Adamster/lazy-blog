using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.CreatePost;

internal sealed class CreatePostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ITagRepository tagRepository)
    : ICommandHandler<CreatePostCommand, PostCreatedResponse>
{
    public async Task<Result<PostCreatedResponse>> Handle(CreatePostCommand request, CancellationToken ct)
    {
        Result<Title> titleResult = Title.Create(request.Title);
        Result<Summary> summaryResult = Summary.Create(request.Summary);
        Result<Body> bodyResult = Body.Create(request.Body);
        List<Tag> tags = await tagRepository.GetTagByIdsAsync(request.Tags, ct);

        if (!tags.Any())
        {
            return Result.Failure<PostCreatedResponse>(DomainErrors.Tag.NotFound(request.Tags.First().ToString()));
        }

        if (await userRepository.GetByIdAsync(request.UserId, ct) is null)
        {
            return Result.Failure<PostCreatedResponse>(DomainErrors.User.NotFound(request.UserId));
        }

        Result<Slug> slugResult = Slug.Create(request.Title.Slugify());

        Guid postId = Guid.NewGuid();

        if (await postRepository.GetBySlugAsync(slugResult.Value, ct) is not null)
        {
            slugResult = Slug.Create($"{postId.ToByteArray().GetHashCode()}-{slugResult.Value.Value}");
        }

        Post post = Post.Create(
            postId,
            titleResult.Value,
            summaryResult.Value,
            slugResult.Value,
            bodyResult.Value,
            request.UserId,
            request.IsPublished,
            tags,
            request.CoverUrl);

        postRepository.Add(post);

        await unitOfWork.SaveChangesAsync(ct);

        return new PostCreatedResponse(post.Id, post.Slug.Value);
    }
}
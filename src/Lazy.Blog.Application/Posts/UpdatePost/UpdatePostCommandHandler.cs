using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.UpdatePost;

public class UpdatePostCommandHandler(
    IPostRepository postRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    ITagRepository tagRepository)
    : ICommandHandler<UpdatePostCommand>
{
    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken ct)
    {
        var post = await postRepository.GetByIdAsync(request.Id, ct);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.Id));
        }

        if (!currentUserContext.IsCurrentUser(post.UserId))
        {
            return Result.Failure(DomainErrors.Post.UnauthorizedPostAccess);
        }

        Result<Title> titleResult = Title.Create(request.Title);
        Result<Summary> summaryResult = Summary.Create(request.Summary);
        Result<Body> bodyResult = Body.Create(request.Body);
        Result<Slug> slugResult = Slug.Create(request.Slug);

        List<Tag> updatedTags = await tagRepository.GetTagByIdsAsync(request.Tags, ct);
        
        if (!updatedTags.Any())
        {
            return Result.Failure(DomainErrors.Tag.NotFound(request.Tags.First().ToString()));
        }
        
        post.Update(
            titleResult.Value,
            summaryResult.Value,
            bodyResult.Value,
            slugResult.Value,
            request.CoverUrl,
            updatedTags,
            request.IsPublished);

        postRepository.Update(post);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private List<Tag> CreateTags(List<TagResponse> tagsToCreate)
    {
        var tags = new List<Tag>();

        if (tags.Any())
        {
            return tags;
        }

        foreach (var requestedTag in tagsToCreate)
        {
            var tagResult = Tag.Create(requestedTag.Tag);
            tags.Add(tagResult.Value);
        }

        return tags;
    }
}
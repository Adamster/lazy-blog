using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
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

        List<Tag> updatedTags = [];

        if (request.Tags.Count != 0)
        {
            updatedTags = await tagRepository.GetTagByIdsAsync(request.Tags, ct);
            tagRepository.Attach(updatedTags);
        }

        post.Update(
            titleResult.Value,
            summaryResult.Value,
            bodyResult.Value,
            slugResult.Value,
            request.CoverUrl,
            request.IsCoverDisplayed,
            updatedTags,
            request.IsPublished);

        postRepository.Update(post);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
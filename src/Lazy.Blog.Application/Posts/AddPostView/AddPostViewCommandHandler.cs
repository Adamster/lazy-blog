using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.AddPostView;

public class AddPostViewCommandHandler : ICommandHandler<AddPostViewCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPostViewCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddPostViewCommand request, CancellationToken ct)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, ct);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.Id));
        }

        post.AddView();

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
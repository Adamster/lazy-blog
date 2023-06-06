using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IPostVoteRepository
{
    void Add(PostVote vote);
    void Delete(PostVote vote);

    Task<PostVote?> GetPostVoteForUserIdAsync(Guid userId, Guid postId, CancellationToken ct);

    void Update(PostVote vote);
}
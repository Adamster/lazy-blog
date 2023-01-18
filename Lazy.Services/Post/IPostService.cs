using Lazy.DataContracts.Post;

namespace Lazy.Services.Post;

public interface IPostService
{
    Task<PostItemDto> CreatePost(CreatePostDto post);

    Task<bool> TogglePostVisibility(Guid postId);

    Task<bool> DeletePost(Guid postId);

    Task VotePost(Guid postId, bool isUpvote);

    Task UpdatePost(UpdatePostDto updatedPost);

    Task<PostItemDto?> GetPostById(Guid postId);

    Task<IList<PostItemDto>> GetPostList(int pageNumber = 0);
}
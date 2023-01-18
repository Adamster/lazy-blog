using Lazy.DataContracts.Post;

namespace Lazy.Services.Post;

public interface IPostService
{
    Task<PostItemDto> CreatePost(CreatePostDto post);

    Task<bool> TogglePostVisibility(Guid postId);

    Task<bool> DeletePost(Guid postId);

    Task VotePost(Guid postId, bool isUpvote);

    Task<bool> UpdatePost(UpdatePostDto updatedPost);

    Task<PostItemDetails> GetPostById(Guid postId);

    Task<IList<PostItemDto>> GetPostList(int pageNumber = 0);
}
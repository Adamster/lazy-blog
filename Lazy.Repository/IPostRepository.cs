using Lazy.DataContracts.Post;
using Lazy.Domain;

namespace Lazy.Repository;

public interface IPostRepository : IRepository<Post>
{
    Task<IList<PostItemDto>> GetPostsAsync(int pageNumber);

    Task<PostItemDto> CreatePost(CreatePostDto postToCreate);

    Task<PostItemDto?> GetPostById(Guid id);

    Task UpdatePost(PostItemDto newUpdatedPost);
}
using Lazy.DataContracts.Post;
using Lazy.Domain;
using Lazy.Infrastructure;
using Mapster;

namespace Lazy.Repository;

public interface IPostRepository : IRepository<Post>
{
    Task<IList<PostItemDto>> GetPostsAsync(int pageNumber);

    Task<PostItemDto> CreatePost(CreatePostDto postToCreate);

}


public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(LazyBlogDbContext lazyBlogDbContext) : base(lazyBlogDbContext)
    {
    }

    public async Task<IList<PostItemDto>> GetPostsAsync(int pageNumber)
    {
        var items =  await GetItems(pageNumber);
        return items.Adapt<IList<PostItemDto>>();
    }

    public async Task<PostItemDto> CreatePost(CreatePostDto post)
    {
        var postToCreate = new Post(post.Title, post.Description, post.Content, post.AuthorId);

        await SaveOrUpdate(postToCreate, CancellationToken.None);

        return new PostItemDto(postToCreate.Id,
            postToCreate.Title,
            postToCreate.Description,
            postToCreate.Author.Name,
            postToCreate.Content,
            postToCreate.LikeCount,
            postToCreate.Comments.Count,
            postToCreate.CreatedAt
        );

    }
}
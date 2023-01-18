using Lazy.DataContracts.Post;
using Lazy.Infrastructure;
using Lazy.Repository;
using Mapster;

namespace Lazy.Services.Post;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostItemDto> CreatePost(CreatePostDto post)
    {
        if (post == null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        //TODO: remove as author id is available from claims
        PostItemDto newPost =
            await _postRepository.CreatePost(post with { AuthorId = Constants.SystemAuthor.SystemAuthorId });
      return  newPost;
    }

    public Task<bool> TogglePostVisibility(Guid postId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePost(Guid postId)
    {
        throw new NotImplementedException();
    }

    public Task VotePost(Guid postId, bool isUpvote)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdatePost(UpdatePostDto updatedPost)
    {
        throw new NotImplementedException();
    }

    public Task<PostItemDetails> GetPostById(Guid postId)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<PostItemDto>> GetPostList(int pageNumber = 0)
    {
        return await _postRepository.GetPostsAsync(pageNumber);
    }
}
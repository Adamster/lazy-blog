using Lazy.DataContracts.Post;
using Lazy.Infrastructure;
using Lazy.Repository;
using Lazy.Services.Exceptions;
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

    public async Task UpdatePost(UpdatePostDto updatedPost)
    {
        PostItemDto? existingPost = await _postRepository.GetPostById(updatedPost.Id);
        if (existingPost == null)
        {
            throw new EntityNotFoundException($"Post with: id {updatedPost.Id} not found");
        }

        var newUpdatedPost = existingPost with
        {
            Title = updatedPost.Title,
            Description = updatedPost.Description,
            Content = updatedPost.Content
        };

        if (existingPost != newUpdatedPost)
        {
            await _postRepository.UpdatePost(newUpdatedPost);
        }
    }

    public async Task<PostItemDto?> GetPostById(Guid postId)
    {
        var post = await _postRepository.GetItemById(postId);
        return post?.Adapt<PostItemDto>();
    }

    public async Task<IList<PostItemDto>> GetPostList(int pageNumber = 0)
    {
        return await _postRepository.GetPostsAsync(pageNumber);
    }
}
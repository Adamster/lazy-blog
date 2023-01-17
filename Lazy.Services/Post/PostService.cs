using Lazy.DataContracts.Post;
using Lazy.Repository;

namespace Lazy.Services.Post;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public Task<PostItemDto> CreatePost(CreatePostDto post)
    {
        throw new NotImplementedException();
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

    public async Task<IList<PostItemDto>> GetPostList(int pageNumber = 1)
    {
        var content =
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Luctus venenatis lectus magna fringilla urna porttitor. Et malesuada fames ac turpis egestas. Duis ultricies lacus sed turpis tincidunt id aliquet. Id eu nisl nunc mi ipsum faucibus vitae aliquet. Sodales neque sodales ut etiam sit amet nisl purus in. Est sit amet facilisis magna etiam tempor orci eu lobortis. Netus et malesuada fames ac turpis egestas maecenas pharetra. Amet aliquam id diam maecenas ultricies mi eget mauris. Dolor sit amet consectetur adipiscing elit pellentesque habitant morbi tristique. Sit amet luctus venenatis lectus magna fringilla urna porttitor rhoncus. Erat velit scelerisque in dictum non consectetur a erat. Et egestas quis ipsum suspendisse. Ut tortor pretium viverra suspendisse. Pellentesque adipiscing commodo elit at imperdiet dui accumsan sit. Malesuada fames ac turpis egestas sed.

Elementum facilisis leo vel fringilla est. Pulvinar sapien et ligula ullamcorper malesuada proin libero nunc. Nisl purus in mollis nunc sed id semper. Nisi vitae suscipit tellus mauris a diam maecenas sed. Felis bibendum ut tristique et egestas quis ipsum. Nulla pellentesque dignissim enim sit amet venenatis. Amet commodo nulla facilisi nullam vehicula ipsum a arcu cursus. Faucibus in ornare quam viverra orci sagittis. Nec nam aliquam sem et tortor consequat id porta nibh. Lorem ipsum dolor sit amet consectetur adipiscing elit duis.";


        var list = new List<PostItemDto>
        {
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),
            new("Some title", "some description", "Adamster",content, 444, 12, DateTimeOffset.Now),

        };
        return await Task.FromResult(list);
        throw new NotImplementedException();
    }
}
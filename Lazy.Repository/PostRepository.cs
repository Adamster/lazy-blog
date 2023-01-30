﻿using Lazy.DataContracts.Post;
using Lazy.Domain;
using Lazy.Infrastructure;
using Mapster;

namespace Lazy.Repository;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(LazyBlogDbContext dbContext) : base(dbContext)
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

    public async Task<PostItemDto?> GetPostById(Guid id)
    {
        Post? post = await GetItemById(id);
        var postDto = post?.Adapt<PostItemDto>();
        return postDto;
        
    }

    public async Task UpdatePost(PostItemDto newUpdatedPost)
    {
        var post = await GetItemById(newUpdatedPost.Id);
        if (post == null)
        {
            throw new ApplicationException($"Post with Id: {newUpdatedPost.Id} not found");
        }

        post.Update(newUpdatedPost.Title, newUpdatedPost.Content, newUpdatedPost.Description);

        await SaveOrUpdate(post, CancellationToken.None);
    }
}
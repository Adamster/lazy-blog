using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.Extensions.Caching.Memory;

namespace Lazy.Persistence.Repositories;

/// <summary>
/// temporary removed cause of multiple issues 
/// </summary>
[Obsolete]
public class CachedPostRepository : IPostRepository
{
    private readonly PostRepository _postRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly HashSet<string> _cacheKeys = new();

    public CachedPostRepository(PostRepository postRepository, IMemoryCache memoryCache)
    {
        _postRepository = postRepository;
        _memoryCache = memoryCache;
    }

    public Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken)
    {
        string key = $"post-{postId}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return _postRepository.GetByIdAsync(postId, cancellationToken);
            });
    }

    public Task<Post?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken)
    {
        string key = $"post-{slug.Value}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return _postRepository.GetBySlugAsync(slug, cancellationToken);
            });
    }

    public Task<IList<Post>> GetPostsAsync(int offset, CancellationToken cancellationToken)
    {
        string key = $"posts-{offset}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return  _postRepository.GetPostsAsync(offset, cancellationToken);
            })!;
    }

    public Task<IList<Post>> GetPostsByTagAsync(Tag tag, CancellationToken cancellationToken)
    {
        string key = $"posts-{tag.Value}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return _postRepository.GetPostsByTagAsync(tag, cancellationToken);
            })!;
    }

    public void Add(Post post)
    {
        InvalidateCache();
        _postRepository.Add(post);
    }

    public void Update(Post post)
    {
        InvalidateCache();
        _postRepository.Update(post);
    }

    public void Delete(Post post)
    {
        InvalidateCache();
        _postRepository.Delete(post);
    } 

    public Task<IList<Post>> GetPostsByUserIdAsync(Guid userId, int offset, CancellationToken cancellationToken)
    {
        string key = $"posts-{userId}/{offset}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return _postRepository.GetPostsByUserIdAsync(userId, offset, cancellationToken);
            })!;
    }

    public Task<IList<Post>> GetPostsByUserNameAsync(UserName userName, int offset, CancellationToken cancellationToken)
    {
        string key = $"posts-user-{userName.Value}-{offset}";

        CollectCacheKey(key);

        return _memoryCache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                return _postRepository.GetPostsByUserNameAsync(userName, offset, cancellationToken);
            })!;
    }

    private void CollectCacheKey(string key)
    {
        _cacheKeys.Add(key);
    }

    private void InvalidateCache()
    {
        foreach (var cacheKey in _cacheKeys)
        {
            _memoryCache.Remove(cacheKey);
        }

        _cacheKeys.Clear();
    }
}
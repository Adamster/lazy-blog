﻿using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPublishedPosts;

public class GetPublishedPostsQueryHandler : IQueryHandler<GetPublishedPostsQuery, List<PublishedPostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPublishedPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<PublishedPostResponse>>> Handle(GetPublishedPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetPosts(request.Offset, cancellationToken);

        List<PublishedPostResponse> response = posts
            .Select(p =>
                new PublishedPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary.Value,
                    p.Body.Value,
                    p.CreatedOnUtc))
            .ToList();

        return response;
    }
}
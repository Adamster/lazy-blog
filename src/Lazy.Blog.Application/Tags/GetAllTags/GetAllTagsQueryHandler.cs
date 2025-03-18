using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Tags.GetAllTags;

public class GetAllTagsQueryHandler(ITagRepository tagRepository) : IQueryHandler<GetAllTagsQuery, List<TagResponse>>
{
    public Task<Result<List<TagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var result = tagRepository.GetAllTags();

        var tagResponseWithPostCount = result
            .Select(t => new TagResponse(t.Id, t.Value, t.Posts.Count(p => p.IsPublished)))
            .ToList();

        return Task.FromResult<Result<List<TagResponse>>>(tagResponseWithPostCount);
    }
}
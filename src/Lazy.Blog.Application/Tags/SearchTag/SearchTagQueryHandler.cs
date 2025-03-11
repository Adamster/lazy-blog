using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Tags.SearchTag;

public class SearchTagQueryHandler : IQueryHandler<SearchTagQuery, List<TagResponse>>
{
    private readonly ITagRepository _tagRepository;

    public SearchTagQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<List<TagResponse>>> Handle(SearchTagQuery request, CancellationToken ct)
    {
        List<Tag> result = await _tagRepository.SearchTagAsync(request.SearchTerm, ct);

        if (!result.Any())
        {
            return Enumerable.Empty<TagResponse>().ToList();
        }
        var response = result
            .Select(x => new TagResponse(x.Id, x.Value, x.Posts.Count))
            .ToList();

        return response;
    }
}
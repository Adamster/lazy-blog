using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Tags.GetAllTags;

public class GetAllTagsQueryHandler : IQueryHandler<GetAllTagsQuery, List<TagResponse>>
{
    private readonly ITagRepository _tagRepository;

    public GetAllTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<List<TagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var result =  await _tagRepository.GetAllTagsAsync(cancellationToken);

        return result.Select(t => new TagResponse(t.Id, t.Value)).ToList();
    }
}
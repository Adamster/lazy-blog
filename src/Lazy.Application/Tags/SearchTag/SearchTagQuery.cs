using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Tags.SearchTag;

public record SearchTagQuery(string SearchTerm) : IQuery<List<TagResponse>>;
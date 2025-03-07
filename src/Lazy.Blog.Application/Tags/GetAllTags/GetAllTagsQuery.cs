using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Tags.GetAllTags;

public record GetAllTagsQuery : IQuery<List<TagResponse>>;
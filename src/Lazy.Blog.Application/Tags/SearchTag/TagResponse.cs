namespace Lazy.Application.Tags.SearchTag;

public record TagResponse(Guid TagId, string Tag, int PostCount);

public record TagPostResponse(Guid TagId, string Tag);
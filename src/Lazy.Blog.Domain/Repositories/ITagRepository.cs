using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> SearchTagAsync(string searchTerm, CancellationToken ct);
    
    IQueryable<Tag> GetAllTags();

    Tag? GetTagByValue(string tagValue);

    Tag? GetTagById(Guid tagId);
    Task<List<Tag>> GetTagByIdsAsync(IEnumerable<Guid> tagIds, CancellationToken ct);
    void Update(Tag tag);
    void Attach(List<Tag> requestTags);
}
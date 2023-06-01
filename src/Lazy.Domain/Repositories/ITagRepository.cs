using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> SearchTagAsync(string searchTerm, CancellationToken cancellationToken);

    Tag? GetTagByValue(string tagValue);

    Tag? GetTagById(Guid tagId);
    Task<List<Tag>> GetTagByIdsAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken);
    void Update(Tag tag);
}
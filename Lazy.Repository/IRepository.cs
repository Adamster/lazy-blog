using Lazy.Domain;

namespace Lazy.Repository;

public interface IRepository<T> where T : Entity
{
   Task<IList<T>> GetItems(int pageIndex = 1, int pageSize = 100);

   Task<T?> GetItemById(Guid id);

   Task<T> SaveOrUpdate(T item, CancellationToken ct);

   Task<bool> DeleteItem(Guid id, CancellationToken ct);
}
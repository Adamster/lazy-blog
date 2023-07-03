using System.Data;

namespace Lazy.Domain.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);

    IDbTransaction BeginTransaction();
}
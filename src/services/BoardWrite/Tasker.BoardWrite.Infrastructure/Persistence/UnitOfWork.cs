using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardWriteDbContext _db;

    public UnitOfWork(BoardWriteDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
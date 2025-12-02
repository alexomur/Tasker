using System.Threading;
using System.Threading.Tasks;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardWriteDbContext _dbContext;

    public UnitOfWork(BoardWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}
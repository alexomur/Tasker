using Tasker.Auth.Application.Abstractions.Persistence;

namespace Tasker.Auth.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _db;

    public UnitOfWork(AuthDbContext db)
    {
        _db = db;
    }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
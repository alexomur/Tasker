using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasker.Core;

namespace Tasker.Data.Repositories;

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly TaskerDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(TaskerDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _set = _db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync([id], ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default)
    {
        return await _set.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _set.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _set.FindAsync([id], ct);
        if (entity == null)
        {
            return;
        }
        
        _set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _set.AsNoTracking().AnyAsync(predicate, ct);
    }

    public virtual async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _set.CountAsync(ct);
    }
}

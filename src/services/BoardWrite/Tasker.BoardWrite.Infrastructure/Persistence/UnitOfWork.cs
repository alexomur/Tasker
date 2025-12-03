using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardWriteDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(BoardWriteDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                _logger.LogError(
                    ex,
                    "DbUpdateConcurrencyException for entity {EntityType} with state {State}. Key: {KeyValues}",
                    entry.Metadata.ClrType.Name,
                    entry.State,
                    string.Join(", ",
                        entry.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))
                );
            }

            throw;
        }
    }
}
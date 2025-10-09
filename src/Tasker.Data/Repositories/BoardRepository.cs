using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasker.Core.Boards;

namespace Tasker.Data.Repositories;

public class BoardRepository : Repository<Board>, IBoardRepository
{
    public BoardRepository(TaskerDbContext db) : base(db)
    {
        
    }

    public async Task<Board?> GetByIdWithGraphAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Board?> GetByIdWithGraphNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Card?> FindCardAsync(Guid boardId, Guid cardId, CancellationToken ct = default)
    {
        return (await GetByIdWithGraphAsync(boardId, ct))?.Columns.SelectMany(c => c.Cards).FirstOrDefault(card => card.Id == cardId);
    }

    public async Task<Card> AddCardAsync(Guid boardId, Guid columnId, string title, string? description = null, CancellationToken ct = default)
    {
        var board = await GetByIdWithGraphAsync(boardId, ct)
                    ?? throw new KeyNotFoundException($"Board {boardId} not found");

        var column = board.GetColumn(columnId) ?? throw new KeyNotFoundException($"Column {columnId} not found on board {boardId}");

        var card = column.AddCard(title, description);

        if (card.Id == Guid.Empty)
        {
            card.Id = Guid.NewGuid();
        }

        await _db.SaveChangesAsync(ct);

        return card;
    }

    public async Task<Card> MoveCardAsync(Guid boardId, Guid cardId, Guid toColumnId, int? insertIndex = null, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var board = await GetByIdWithGraphAsync(boardId, ct)
                        ?? throw new KeyNotFoundException($"Board {boardId} not found");

            var moved = board.MoveCard(cardId, toColumnId, insertIndex);

            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return moved;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> RemoveCardAsync(Guid boardId, Guid cardId, CancellationToken ct = default)
    {
        var board = await GetByIdWithGraphAsync(boardId, ct)
                    ?? throw new KeyNotFoundException($"Board {boardId} not found");

        var sourceColumn = board.Columns.FirstOrDefault(c => c.Cards.Any(card => card.Id == cardId));
        if (sourceColumn is null)
        {
            return false;
        }

        var card = sourceColumn.GetCard(cardId);
        if (card == null)
        {
            return false;
        }

        var removedFromCollection = sourceColumn.RemoveCard(cardId);
        if (!removedFromCollection)
        {
            return false;
        }

        _db.Cards.Remove(card);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Column> AddColumnAsync(Guid boardId, string title, string? description = null, CancellationToken ct = default)
    {
        var board = await GetByIdWithGraphAsync(boardId, ct)
                    ?? throw new KeyNotFoundException($"Board {boardId} not found");

        var column = board.AddColumn(title, description);
        if (column.Id == Guid.Empty)
        {
            column.Id = Guid.NewGuid();
        }

        await _db.SaveChangesAsync(ct);
        return column;
    }

    public async Task<bool> RemoveColumnAsync(Guid boardId, Guid columnId, CancellationToken ct = default)
    {
        var board = await GetByIdWithGraphAsync(boardId, ct)
                    ?? throw new KeyNotFoundException($"Board {boardId} not found");

        var column = board.GetColumn(columnId);
        if (column == null)
        {
            return false;
        }

        var removed = board.RemoveColumn(columnId);
        if (!removed)
        {
            return false;
        }

        _db.Columns.Remove(column);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

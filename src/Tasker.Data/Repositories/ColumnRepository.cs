using Microsoft.EntityFrameworkCore;
using Tasker.Core.Boards;

namespace Tasker.Data.Repositories;

public class ColumnRepository : Repository<Column>, IColumnRepository
{
    public ColumnRepository(TaskerDbContext db) : base(db)
    {
    }

    public async  Task<IReadOnlyList<Column>> ListByBoardIdAsync(Guid boardId, CancellationToken ct = default)
    {
        var columns = await _db.Columns
            .AsNoTracking()
            .Where(column => EF.Property<Guid>(column, "BoardId") == boardId)
            .ToListAsync(ct);

        return columns;
    }

    public async Task<Column?> GetByIdWithCardsAsync(Guid columnId, CancellationToken ct = default)
    {
        var columnWithCards = await _db.Columns
            .Include(column => column.Cards)
            .FirstOrDefaultAsync(column => column.Id == columnId, ct);

        return columnWithCards;
    }

    public async Task<Column?> UpdateTitleAsync(Guid columnId, string newTitle, CancellationToken ct = default)
    {
        var columnEntity = await _db.Columns.FindAsync([columnId], ct);

        if (columnEntity == null)
        {
            return null;
        }

        columnEntity.Title = newTitle;

        await _db.SaveChangesAsync(ct);
        return columnEntity;
    }
}
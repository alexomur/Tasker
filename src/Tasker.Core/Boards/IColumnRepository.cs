namespace Tasker.Core.Boards;

public interface IColumnRepository : IRepository<Column>
{
    Task<IReadOnlyList<Column>> ListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<Column?> GetByIdWithCardsAsync(Guid columnId, CancellationToken ct = default);
}
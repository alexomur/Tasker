namespace Tasker.Core.Boards;

public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetByIdWithGraphAsync(Guid id, CancellationToken ct = default);

    Task<Board?> GetByIdWithGraphNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<Card?> FindCardAsync(Guid boardId, Guid cardId, CancellationToken ct = default);

    Task<Card> AddCardAsync(Guid boardId, Guid columnId, string title, string? description = null, CancellationToken ct = default);

    Task<Card> MoveCardAsync(Guid boardId, Guid cardId, Guid toColumnId, int? insertIndex = null, CancellationToken ct = default);

    Task<bool> RemoveCardAsync(Guid boardId, Guid cardId, CancellationToken ct = default);
}
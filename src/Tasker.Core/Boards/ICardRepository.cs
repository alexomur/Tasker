namespace Tasker.Core.Boards;

public interface ICardRepository : IRepository<Card>
{
    Task<IReadOnlyList<Card>> ListByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<Card?> UpdateFieldsAsync(Guid cardId, string? title = null, string? description = null, CancellationToken ct = default);

    Task<bool> DeleteByIdAsync(Guid cardId, CancellationToken ct = default);
}
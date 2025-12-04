namespace Tasker.Shared.Kernel.Abstractions.ReadModel;

public interface IBoardSnapshotStore
{
    /// <summary>
    /// Попробовать получить JSON-снапшот доски из read-store.
    /// </summary>
    Task<string?> TryGetAsync(Guid boardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать или обновить снапшот доски с TTL.
    /// </summary>
    Task UpsertAsync(Guid boardId, string payloadJson, int ttlSeconds, CancellationToken cancellationToken = default);
}
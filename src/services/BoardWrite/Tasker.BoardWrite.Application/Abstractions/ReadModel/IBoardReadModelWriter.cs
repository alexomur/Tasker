namespace Tasker.BoardWrite.Application.Abstractions.ReadModel;

public interface IBoardReadModelWriter
{
    /// <summary>
    /// Пересобрать и сохранить снапшот доски в read-store.
    /// </summary>
    Task RefreshBoardAsync(Guid boardId, CancellationToken cancellationToken = default);
}
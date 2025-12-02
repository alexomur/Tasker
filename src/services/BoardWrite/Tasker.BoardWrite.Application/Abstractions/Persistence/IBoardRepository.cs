using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий для агрегата Board.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Возвращает доску по идентификатору или null, если не найдена.
    /// </summary>
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Добавляет новую доску в контекст. Сохранение выполняется через IUnitOfWork.
    /// </summary>
    Task AddAsync(Board board, CancellationToken ct = default);
}
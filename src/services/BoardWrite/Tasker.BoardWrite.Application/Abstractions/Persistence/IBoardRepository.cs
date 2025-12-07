using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий для агрегата Board.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Возвращает доску по идентификатору с полным набором связанных сущностей без трекинга изменений.
    /// </summary>
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Возвращает доску по идентификатору с полным набором связанных сущностей и включённым трекингом.
    /// Используется для сценариев изменения агрегата.
    /// </summary>
    Task<Board?> GetByIdAsTrackingAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Регистрирует новую доску в контексте.
    /// Фактическое сохранение происходит при вызове UnitOfWork.SaveChangesAsync().
    /// </summary>
    Task AddAsync(Board board, CancellationToken ct = default);

    /// <summary>
    /// Регистрирует новую сущность, принадлежащую доске (колонка, метка, карточка, участник и т.д.),
    /// как <see cref="Microsoft.EntityFrameworkCore.EntityState.Added"/> в контексте.
    /// Позволяет использовать доменные методы агрегата и при этом корректно вставлять новые сущности в БД.
    /// </summary>
    Task AddEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        where TEntity : Entity;

    /// <summary>
    /// Возвращает доски, в которых указанный пользователь является активным участником.
    /// </summary>
    Task<IReadOnlyCollection<Board>> GetBoardsForUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Помечает доску на удаление. Фактическое удаление произойдет при SaveChangesAsync().
    /// Ожидается, что каскадные правила в БД/EF удалят связанные сущности.
    /// </summary>
    Task RemoveAsync(Board board, CancellationToken ct = default);

    /// <summary>
    /// Помечает произвольную сущность агрегата на удаление
    /// (колонку, карточку, метку, участника и т.п.).
    /// </summary>
    Task RemoveEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : Entity;
}
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
}
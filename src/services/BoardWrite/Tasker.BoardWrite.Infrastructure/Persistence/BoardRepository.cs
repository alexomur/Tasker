using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

/// <summary>
/// Репозиторий для агрегата Board. Загружает доску с колонками, участниками, метками и карточками.
/// </summary>
public sealed class BoardRepository : IBoardRepository
{
    private readonly BoardWriteDbContext _db;

    public BoardRepository(BoardWriteDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Возвращает доску по идентификатору с полным набором связанных сущностей и трекингом.
    /// Используется для сценариев изменения агрегата (write).
    /// </summary>
    public Task<Board?> GetByIdAsTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Boards
            .AsTracking()
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .Include(b => b.Cards)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    /// <summary>
    /// Возвращает доску по идентификатору с полным набором связанных сущностей без трекинга.
    /// Подходит для read-only сценариев.
    /// </summary>
    public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .Include(b => b.Cards)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    /// <summary>
    /// Добавляет новую доску в контекст. Фактическое сохранение происходит при вызове UnitOfWork.SaveChangesAsync().
    /// </summary>
    public Task AddAsync(Board board, CancellationToken ct = default)
    {
        return _db.Boards.AddAsync(board, ct).AsTask();
    }

    /// <summary>
    /// Регистрирует новую сущность, принадлежащую доске (колонка, метка, карточка, участник и т.д.),
    /// как Added в контексте.
    /// </summary>
    public Task AddEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        where TEntity : Entity
    {
        return _db.Set<TEntity>().AddAsync(entity, ct).AsTask();
    }
}

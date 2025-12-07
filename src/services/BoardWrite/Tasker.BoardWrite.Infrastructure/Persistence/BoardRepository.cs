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
            .AsSplitQuery()
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .Include(b => b.Cards)
                .ThenInclude(c => c.Labels)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    /// <summary>
    /// Возвращает доску по идентификатору с полным набором связанных сущностей без трекинга.
    /// Подходит для read-only сценариев.
    /// </summary>
    public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Boards
            .AsSplitQuery()
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .Include(b => b.Cards)
                .ThenInclude(c => c.Labels)
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

    /// <summary>
    /// Возвращает доски, в которых пользователь является активным участником.
    /// Активность определяем через LeftAt == null, т.к. IsActive не мапится в БД.
    /// </summary>
    public async Task<IReadOnlyCollection<Board>> GetBoardsForUserAsync(Guid userId, CancellationToken ct)
    {
        var boards = await _db.Boards
            .Include(b => b.Members)
            .Where(b => b.Members.Any(m => m.UserId == userId && m.LeftAt == null))
            .ToListAsync(ct);

        return boards;
    }

    /// <summary>
    /// Помечает доску на удаление. Фактическое удаление произойдет при SaveChangesAsync().
    /// Ожидается, что каскадные правила в БД/EF удалят связанные сущности.
    /// </summary>
    public Task RemoveAsync(Board board, CancellationToken ct = default)
    {
        _db.Boards.Remove(board);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Помечает произвольную сущность агрегата на удаление
    /// (колонку, карточку, метку, участника и т.п.).
    /// </summary>
    public Task RemoveEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        where TEntity : Entity
    {
        _db.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
}

using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Boards;

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
    /// Возвращает доску по идентификатору с полным набором связанных сущностей.
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
}
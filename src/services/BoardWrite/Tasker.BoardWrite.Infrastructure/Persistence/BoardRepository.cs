using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

public class BoardRepository : IBoardRepository
{
    private readonly BoardWriteDbContext _db;

    public BoardRepository(BoardWriteDbContext db)
    {
        _db = db;
    }

    public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task AddAsync(Board board, CancellationToken ct = default)
    {
        await _db.Boards.AddAsync(board, ct);
    }
}
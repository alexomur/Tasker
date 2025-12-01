using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Abstractions.Persistence;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Board board, CancellationToken ct = default);
}
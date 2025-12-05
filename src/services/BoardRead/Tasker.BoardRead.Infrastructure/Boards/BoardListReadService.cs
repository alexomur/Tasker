using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Application.Boards.Views;
using Tasker.BoardWrite.Infrastructure;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardRead.Infrastructure.Boards;

using ReadBoardMemberRole = BoardMemberRole;

/// <summary>
/// Чтение списка досок текущего пользователя напрямую из MySQL (BoardWriteDbContext).
/// </summary>
public sealed class BoardListReadService : IBoardListReadService
{
    private readonly BoardWriteDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<BoardListReadService> _logger;

    public BoardListReadService(
        BoardWriteDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<BoardListReadService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<BoardView>> GetMyBoardsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("Текущий пользователь не определён.");
        }

        var userId = _currentUser.UserId.Value;

        _logger.LogDebug("Loading boards for user {UserId}", userId);

        var boards = await _dbContext.Boards
            .AsNoTracking()
            .Include(b => b.Members)
            .Where(b => b.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);

        var result = boards
            .Select(board =>
            {
                var member = board.Members
                    .FirstOrDefault(m => m.UserId == userId && m.IsActive);

                if (member is null)
                {
                    // Теоретически не должно случиться, учитывая фильтр выше.
                    _logger.LogWarning(
                        "Board {BoardId} selected for user {UserId}, but active membership not found",
                        board.Id,
                        userId);

                    // По умолчанию считаем Viewer, чтобы не падать.
                    return new BoardView(
                        Id: board.Id,
                        Title: board.Title,
                        Description: board.Description,
                        OwnerUserId: board.OwnerUserId,
                        IsArchived: board.IsArchived,
                        CreatedAt: board.CreatedAt,
                        UpdatedAt: board.UpdatedAt,
                        MyRole: ReadBoardMemberRole.Viewer);
                }

                return new BoardView(
                    Id: board.Id,
                    Title: board.Title,
                    Description: board.Description,
                    OwnerUserId: board.OwnerUserId,
                    IsArchived: board.IsArchived,
                    CreatedAt: board.CreatedAt,
                    UpdatedAt: board.UpdatedAt,
                    MyRole: (ReadBoardMemberRole)(int)member.Role);
            })
            .OrderBy(b => b.Title)
            .ToList();

        return result;
    }
}

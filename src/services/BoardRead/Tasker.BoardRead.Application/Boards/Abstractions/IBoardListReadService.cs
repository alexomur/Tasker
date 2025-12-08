using Tasker.BoardRead.Domain.BoardViews;

namespace Tasker.BoardRead.Application.Boards.Abstractions;

/// <summary>
/// Сервис чтения списка досок для текущего пользователя.
/// </summary>
public interface IBoardListReadService
{
    /// <summary>
    /// Вернуть список досок, в которых текущий пользователь является активным участником.
    /// </summary>
    Task<IReadOnlyCollection<BoardView>> GetMyBoardsAsync(
        CancellationToken cancellationToken = default);
}
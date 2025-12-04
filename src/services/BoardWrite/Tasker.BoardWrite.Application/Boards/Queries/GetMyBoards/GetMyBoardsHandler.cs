using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.Shared.Kernel.Abstractions;
using Tasker.Shared.Kernel.Abstractions.Security;

namespace Tasker.BoardWrite.Application.Boards.Queries.GetMyBoards;

/// <summary>
/// Обработчик запроса получения списка досок текущего пользователя.
/// </summary>
public sealed class GetMyBoardsHandler
    : IRequestHandler<GetMyBoardsQuery, IReadOnlyCollection<MyBoardListItemResult>>
{
    private readonly IBoardRepository _boards;
    
    private readonly ICurrentUser _currentUser;

    public GetMyBoardsHandler(IBoardRepository boards, ICurrentUser currentUser)
    {
        _boards = boards;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<MyBoardListItemResult>> Handle(
        GetMyBoardsQuery request,
        CancellationToken ct)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("Текущий пользователь не определён.");
        }

        var userId = _currentUser.UserId.Value;

        var boards = await _boards.GetBoardsForUserAsync(userId, ct);

        var result = boards
            .Select(board =>
            {
                var member = board.Members
                    .FirstOrDefault(m => m.UserId == userId && m.IsActive);

                if (member is null)
                {
                    // Теоретически не должно случиться, учитывая фильтр в репозитории.
                    throw new InvalidOperationException(
                        $"Пользователь {userId} не является активным участником доски {board.Id}.");
                }

                return new MyBoardListItemResult(
                    Id: board.Id,
                    Title: board.Title,
                    Description: board.Description,
                    OwnerUserId: board.OwnerUserId,
                    IsArchived: board.IsArchived,
                    CreatedAt: board.CreatedAt,
                    UpdatedAt: board.UpdatedAt,
                    MyRole: member.Role);
            })
            .OrderBy(b => b.Title)
            .ToList();

        return result;
    }
}

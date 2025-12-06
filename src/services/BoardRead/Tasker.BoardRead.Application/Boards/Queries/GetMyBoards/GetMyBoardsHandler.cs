using MediatR;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Application.Boards.Views;

namespace Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;

/// <summary>
/// Обработчик запроса списка досок текущего пользователя.
/// </summary>
public sealed class GetMyBoardsHandler
    : IRequestHandler<GetMyBoardsQuery, IReadOnlyCollection<BoardView>>
{
    private readonly IBoardListReadService _boards;

    public GetMyBoardsHandler(IBoardListReadService boards)
    {
        _boards = boards;
    }

    public Task<IReadOnlyCollection<BoardView>> Handle(
        GetMyBoardsQuery request,
        CancellationToken ct)
    {
        return _boards.GetMyBoardsAsync(ct);
    }
}
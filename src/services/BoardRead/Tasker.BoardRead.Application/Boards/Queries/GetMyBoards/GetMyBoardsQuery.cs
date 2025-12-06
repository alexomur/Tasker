using MediatR;
using Tasker.BoardRead.Application.Boards.Views;

namespace Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;

/// <summary>
/// Запрос списка досок текущего пользователя (read-модель).
/// </summary>
public sealed record GetMyBoardsQuery
    : IRequest<IReadOnlyCollection<BoardView>>;
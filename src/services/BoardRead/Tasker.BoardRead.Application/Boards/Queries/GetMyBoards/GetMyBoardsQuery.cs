using MediatR;
using Tasker.BoardRead.Domain.BoardViews;

namespace Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;

/// <summary>
/// Запрос списка досок текущего пользователя (read-модель).
/// </summary>
public sealed record GetMyBoardsQuery
    : IRequest<IReadOnlyCollection<BoardView>>;
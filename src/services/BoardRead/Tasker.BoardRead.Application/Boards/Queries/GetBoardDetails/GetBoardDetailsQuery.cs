using MediatR;
using Tasker.BoardRead.Application.Boards.Views;

namespace Tasker.BoardRead.Application.Boards.Queries.GetBoardDetails;

/// <summary>
/// Запрос на получение полной информации о доске (read-модель).
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
public sealed record GetBoardDetailsQuery(Guid BoardId)
    : IRequest<BoardDetailsView?>;
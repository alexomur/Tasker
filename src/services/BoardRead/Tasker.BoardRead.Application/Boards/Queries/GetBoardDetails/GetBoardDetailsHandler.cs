using MediatR;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Domain.BoardViews;

namespace Tasker.BoardRead.Application.Boards.Queries.GetBoardDetails;

/// <summary>
/// Обработчик запроса получения полной информации о доске.
/// Делегирует чтение в IBoardDetailsReadService (Cassandra + MySQL fallback).
/// </summary>
public sealed class GetBoardDetailsHandler
    : IRequestHandler<GetBoardDetailsQuery, BoardDetailsView?>
{
    private readonly IBoardDetailsReadService _boards;

    public GetBoardDetailsHandler(IBoardDetailsReadService boards)
    {
        _boards = boards;
    }

    public Task<BoardDetailsView?> Handle(
        GetBoardDetailsQuery request,
        CancellationToken ct)
    {
        return _boards.GetBoardAsync(request.BoardId, ct);
    }
}
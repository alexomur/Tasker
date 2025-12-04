using System.Text.Json;
using MediatR;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Boards.Queries.GetBoardDetails;
using Tasker.Shared.Kernel.Abstractions.ReadModel;

namespace Tasker.BoardWrite.Infrastructure.ReadModel;

public sealed class BoardReadModelWriter : IBoardReadModelWriter
{
    private readonly IMediator _mediator;
    private readonly IBoardSnapshotStore _snapshots;

    private const int TtlSeconds = 24 * 60 * 60; // 24 часа

    public BoardReadModelWriter(IMediator mediator, IBoardSnapshotStore snapshots)
    {
        _mediator = mediator;
        _snapshots = snapshots;
    }

    public async Task RefreshBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        // Берём тот же DTO, который отдаёт /api/v1/boards/{id}
        var boardDetails = await _mediator.Send(new GetBoardDetailsQuery(boardId), cancellationToken);
        if (boardDetails is null)
        {
            // Доска могла быть удалена — можно удалить снапшот, но пока просто игнорируем
            return;
        }

        var json = JsonSerializer.Serialize(boardDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await _snapshots.UpsertAsync(boardId, json, TtlSeconds, cancellationToken);
    }
}
using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddColumn;

/// <summary>
/// Обработчик команды добавления колонки.
/// </summary>
public sealed class AddColumnHandler
    : IRequestHandler<AddColumnCommand, AddColumnResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;

    public AddColumnHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
    }

    public async Task<AddColumnResult> Handle(AddColumnCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, ct);

        var now = DateTimeOffset.UtcNow;

        var column = board.AddColumn(
            title: cmd.Title,
            now: now,
            description: cmd.Description);

        await _boards.AddEntityAsync(column, ct);

        await _uow.SaveChangesAsync(ct);

        return new AddColumnResult(
            ColumnId: column.Id,
            Title: column.Title,
            Description: column.Description,
            Order: column.Order);
    }
}
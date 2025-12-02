using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
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

    public AddColumnHandler(IBoardRepository boards, IUnitOfWork uow)
    {
        _boards = boards;
        _uow = uow;
    }

    public async Task<AddColumnResult> Handle(AddColumnCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsync(cmd.BoardId, ct);
        if (board is null)
            throw new BoardNotFoundException(cmd.BoardId);

        var now = DateTimeOffset.UtcNow;

        var column = board.AddColumn(
            title: cmd.Title,
            now: now,
            description: cmd.Description);

        await _uow.SaveChangesAsync(ct);

        return new AddColumnResult(
            ColumnId: column.Id,
            Title: column.Title,
            Description: column.Description,
            Order: column.Order);
    }
}
using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteColumn;

public sealed class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardReadModelWriter _boardReadModelWriter;

    public DeleteColumnHandler(IBoardRepository boards, IUnitOfWork uow, IBoardReadModelWriter boardReadModelWriter)
    {
        _boards = boards;
        _uow = uow;
        _boardReadModelWriter = boardReadModelWriter;
    }

    public async Task Handle(DeleteColumnCommand cmd, CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        var now = DateTimeOffset.UtcNow;

        board.RemoveColumn(cmd.ColumnId, now);

        await _uow.SaveChangesAsync(cancellationToken);;
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, cancellationToken);
    }
}
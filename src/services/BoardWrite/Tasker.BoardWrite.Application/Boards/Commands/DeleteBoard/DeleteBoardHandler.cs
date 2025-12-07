using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteBoard;

public sealed class DeleteBoardHandler : IRequestHandler<DeleteBoardCommand>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardReadModelWriter _boardReadModelWriter;

    public DeleteBoardHandler(IBoardRepository boards, IUnitOfWork uow, IBoardReadModelWriter boardReadModelWriter)
    {
        _boards = boards;
        _uow = uow;
        _boardReadModelWriter = boardReadModelWriter;
    }

    public async Task Handle(DeleteBoardCommand cmd, CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boards.RemoveAsync(board, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, cancellationToken);
    }
}
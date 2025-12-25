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
    private readonly ICurrentUser _currentUser;

    public DeleteBoardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardReadModelWriter boardReadModelWriter,
        ICurrentUser currentUser)
    {
        _boards = boards;
        _uow = uow;
        _boardReadModelWriter = boardReadModelWriter;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteBoardCommand cmd, CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new InvalidOperationException("Текущий пользователь не определён.");
        }

        board.MarkDeleted(_currentUser.UserId.Value, DateTimeOffset.UtcNow);
        await _boards.RemoveAsync(board, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, cancellationToken);
    }
}

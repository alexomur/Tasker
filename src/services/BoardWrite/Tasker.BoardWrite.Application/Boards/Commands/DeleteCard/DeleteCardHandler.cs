using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteCard;

public sealed class DeleteCardHandler : IRequestHandler<DeleteCardCommand>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardReadModelWriter _boardReadModelWriter;
    private readonly ICurrentUser _currentUser;

    public DeleteCardHandler(
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

    public async Task Handle(DeleteCardCommand cmd, CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        var card = board.Cards.FirstOrDefault(c => c.Id == cmd.CardId);
        if (card is null)
        {
            return;
        }

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new InvalidOperationException("Текущий пользователь не определён.");
        }

        var now = DateTimeOffset.UtcNow;

        board.RemoveCard(cmd.CardId, _currentUser.UserId.Value, now);
        await _uow.SaveChangesAsync(cancellationToken);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, cancellationToken);
    }
}

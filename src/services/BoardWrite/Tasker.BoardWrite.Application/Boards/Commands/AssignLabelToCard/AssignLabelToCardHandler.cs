using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.AssignLabelToCard;

public sealed class AssignLabelToCardHandler
    : IRequestHandler<AssignLabelToCardCommand, AssignLabelToCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IBoardAccessService _boardAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardReadModelWriter _readModelWriter;
    private readonly ICurrentUser _currentUser;

    public AssignLabelToCardHandler(
        IBoardRepository boards,
        IBoardAccessService boardAccess,
        IUnitOfWork unitOfWork,
        IBoardReadModelWriter readModelWriter,
        ICurrentUser currentUser)
    {
        _boards = boards;
        _boardAccess = boardAccess;
        _unitOfWork = unitOfWork;
        _readModelWriter = readModelWriter;
        _currentUser = currentUser;
    }

    public async Task<AssignLabelToCardResult> Handle(
        AssignLabelToCardCommand request,
        CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(request.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(request.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, cancellationToken);

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new InvalidOperationException("Текущий пользователь не определён.");
        }

        var now = DateTimeOffset.UtcNow;

        board.AttachLabelToCard(request.CardId, request.LabelId, _currentUser.UserId.Value, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _readModelWriter.RefreshBoardAsync(board.Id, cancellationToken);

        return new AssignLabelToCardResult(
            CardId: request.CardId,
            LabelId: request.LabelId);
    }
}

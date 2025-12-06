using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.UnassignLabelFromCard;

public sealed class UnassignLabelFromCardHandler
    : IRequestHandler<UnassignLabelFromCardCommand, UnassignLabelFromCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IBoardAccessService _boardAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardReadModelWriter _readModelWriter;

    public UnassignLabelFromCardHandler(
        IBoardRepository boards,
        IBoardAccessService boardAccess,
        IUnitOfWork unitOfWork,
        IBoardReadModelWriter readModelWriter)
    {
        _boards = boards;
        _boardAccess = boardAccess;
        _unitOfWork = unitOfWork;
        _readModelWriter = readModelWriter;
    }

    public async Task<UnassignLabelFromCardResult> Handle(
        UnassignLabelFromCardCommand request,
        CancellationToken cancellationToken)
    {
        var board = await _boards.GetByIdAsTrackingAsync(request.BoardId, cancellationToken);
        if (board is null)
        {
            throw new BoardNotFoundException(request.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        board.DetachLabelFromCard(request.CardId, request.LabelId, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _readModelWriter.RefreshBoardAsync(board.Id, cancellationToken);

        return new UnassignLabelFromCardResult(
            CardId: request.CardId,
            LabelId: request.LabelId);
    }
}
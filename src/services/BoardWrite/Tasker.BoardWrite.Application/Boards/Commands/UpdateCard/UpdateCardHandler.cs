using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;
using Tasker.Shared.Kernel.Abstractions.ReadModel;

namespace Tasker.BoardWrite.Application.Boards.Commands.UpdateCard;

/// <summary>
/// Обработчик команды обновления карточки.
/// </summary>
public sealed class UpdateCardHandler
    : IRequestHandler<UpdateCardCommand, UpdateCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;
    private readonly IBoardReadModelWriter _boardReadModelWriter;

    public UpdateCardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess, IBoardReadModelWriter boardReadModelWriter)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
        _boardReadModelWriter = boardReadModelWriter;
    }

    public async Task<UpdateCardResult> Handle(UpdateCardCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, ct);

        var card = board.Cards.FirstOrDefault(c => c.Id == cmd.CardId);
        if (card is null)
        {
            throw new CardNotFoundException(cmd.CardId);
        }

        var now = DateTimeOffset.UtcNow;

        card.Rename(cmd.Title, now);
        card.ChangeDescription(cmd.Description, now);

        await _uow.SaveChangesAsync(ct);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, ct);

        return new UpdateCardResult(card.Id);
    }
}
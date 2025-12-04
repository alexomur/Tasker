using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.UnassignMemberFromCard;

/// <summary>
/// Обработчик команды снятия участника с роли исполнителя по карточке.
/// </summary>
public sealed class UnassignMemberFromCardHandler
    : IRequestHandler<UnassignMemberFromCardCommand, UnassignMemberFromCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;

    public UnassignMemberFromCardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
    }

    public async Task<UnassignMemberFromCardResult> Handle(UnassignMemberFromCardCommand cmd, CancellationToken ct)
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

        card.UnassignUser(cmd.UserId, now);

        await _uow.SaveChangesAsync(ct);

        return new UnassignMemberFromCardResult(
            CardId: card.Id,
            UserId: cmd.UserId);
    }
}
using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.MoveCard;

/// <summary>
/// Обработчик команды перемещения карточки.
/// </summary>
public sealed class MoveCardHandler
    : IRequestHandler<MoveCardCommand, MoveCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;
    private readonly IBoardReadModelWriter _boardReadModelWriter;
    private readonly ICurrentUser _currentUser;

    public MoveCardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess,
        IBoardReadModelWriter boardReadModelWriter,
        ICurrentUser currentUser)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
        _boardReadModelWriter = boardReadModelWriter;
        _currentUser = currentUser;
    }

    public async Task<MoveCardResult> Handle(MoveCardCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, ct);

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new InvalidOperationException("Текущий пользователь не определён.");
        }

        var card = board.Cards.FirstOrDefault(c => c.Id == cmd.CardId);
        if (card is null)
        {
            throw new CardNotFoundException(cmd.CardId);
        }

        var targetColumn = board.Columns.FirstOrDefault(c => c.Id == cmd.TargetColumnId);
        if (targetColumn is null)
        {
            throw new ColumnNotFoundException(cmd.TargetColumnId);
        }

        var now = DateTimeOffset.UtcNow;

        // Находим максимальный Order в целевой колонке и ставим карточку в конец.
        var existingOrders = board.Cards
            .Where(c => c.ColumnId == cmd.TargetColumnId)
            .Select(c => c.Order)
            .ToArray();

        var newOrder = existingOrders.Length == 0
            ? 0
            : existingOrders.Max() + 1;

        card.MoveToColumn(cmd.TargetColumnId, newOrder, _currentUser.UserId.Value, now);

        await _uow.SaveChangesAsync(ct);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, ct);

        return new MoveCardResult(
            CardId: card.Id,
            ColumnId: card.ColumnId,
            Order: card.Order);
    }
}

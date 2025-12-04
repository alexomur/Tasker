using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateCard;

/// <summary>
/// Обработчик команды создания карточки.
/// </summary>
public sealed class CreateCardHandler
    : IRequestHandler<CreateCardCommand, CreateCardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;
    private readonly ICurrentUser _currentUser;
    private readonly IBoardReadModelWriter _boardReadModelWriter;

    public CreateCardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess,
        ICurrentUser currentUser, IBoardReadModelWriter boardReadModelWriter)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
        _currentUser = currentUser;
        _boardReadModelWriter = boardReadModelWriter;
    }

    public async Task<CreateCardResult> Handle(CreateCardCommand cmd, CancellationToken ct)
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

        var createdByUserId = _currentUser.UserId.Value;
        var now = DateTimeOffset.UtcNow;

        var card = board.CreateCard(
            columnId: cmd.ColumnId,
            title: cmd.Title,
            createdByUserId: createdByUserId,
            now: now,
            description: cmd.Description,
            dueDate: cmd.DueDate);

        await _boards.AddEntityAsync(card, ct);
        await _uow.SaveChangesAsync(ct);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, ct);

        return new CreateCardResult(
            CardId: card.Id,
            ColumnId: card.ColumnId,
            Order: card.Order);
    }
}

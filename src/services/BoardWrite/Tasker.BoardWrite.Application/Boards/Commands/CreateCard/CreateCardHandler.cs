using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
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

    public CreateCardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
    }

    public async Task<CreateCardResult> Handle(CreateCardCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, ct);

        var now = DateTimeOffset.UtcNow;

        var card = board.CreateCard(
            columnId: cmd.ColumnId,
            title: cmd.Title,
            createdByUserId: cmd.CreatedByUserId,
            now: now,
            description: cmd.Description,
            dueDate: cmd.DueDate);

        await _boards.AddEntityAsync(card, ct);

        await _uow.SaveChangesAsync(ct);

        return new CreateCardResult(
            CardId: card.Id,
            ColumnId: card.ColumnId,
            Order: card.Order);
    }
}
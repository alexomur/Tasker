using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
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

    public CreateCardHandler(IBoardRepository boards, IUnitOfWork uow)
    {
        _boards = boards;
        _uow = uow;
    }

    public async Task<CreateCardResult> Handle(CreateCardCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsync(cmd.BoardId, ct);
        if (board is null)
            throw new BoardNotFoundException(cmd.BoardId);

        var now = DateTimeOffset.UtcNow;

        var card = board.CreateCard(
            columnId: cmd.ColumnId,
            title: cmd.Title,
            createdByUserId: cmd.CreatedByUserId,
            now: now,
            description: cmd.Description,
            dueDate: cmd.DueDate);

        await _uow.SaveChangesAsync(ct);

        return new CreateCardResult(
            CardId: card.Id,
            ColumnId: card.ColumnId,
            Order: card.Order);
    }
}
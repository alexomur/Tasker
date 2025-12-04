using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateLabel;

/// <summary>
/// Обработчик команды добавления метки.
/// </summary>
public sealed class CreateLabelHandler
    : IRequestHandler<CreateLabelCommand, AddLabelResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly IBoardAccessService _boardAccess;
    private readonly IBoardReadModelWriter _boardReadModelWriter;

    public CreateLabelHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        IBoardAccessService boardAccess, IBoardReadModelWriter boardReadModelWriter)
    {
        _boards = boards;
        _uow = uow;
        _boardAccess = boardAccess;
        _boardReadModelWriter = boardReadModelWriter;
    }

    public async Task<AddLabelResult> Handle(CreateLabelCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(cmd.BoardId);
        }

        await _boardAccess.EnsureCanWriteBoardAsync(board.Id, ct);

        var label = board.AddLabel(cmd.Title, cmd.Color, cmd.Description);

        await _boards.AddEntityAsync(label, ct);

        await _uow.SaveChangesAsync(ct);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, ct);

        return new AddLabelResult(
            LabelId: label.Id,
            Title: label.Title,
            Color: label.Color,
            Description: label.Description);
    }
}
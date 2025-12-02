using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddLabel;

/// <summary>
/// Обработчик команды добавления метки.
/// </summary>
public sealed class AddLabelHandler
    : IRequestHandler<AddLabelCommand, AddLabelResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;

    public AddLabelHandler(IBoardRepository boards, IUnitOfWork uow)
    {
        _boards = boards;
        _uow = uow;
    }

    public async Task<AddLabelResult> Handle(AddLabelCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsync(cmd.BoardId, ct);
        if (board is null)
            throw new BoardNotFoundException(cmd.BoardId);

        var label = board.AddLabel(cmd.Title, cmd.Color, cmd.Description);

        await _uow.SaveChangesAsync(ct);

        return new AddLabelResult(
            LabelId: label.Id,
            Title: label.Title,
            Color: label.Color,
            Description: label.Description);
    }
}
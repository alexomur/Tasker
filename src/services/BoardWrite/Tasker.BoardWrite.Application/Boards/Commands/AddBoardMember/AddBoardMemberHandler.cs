using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddBoardMember;

/// <summary>
/// Обработчик команды добавления участника.
/// </summary>
public sealed class AddBoardMemberHandler
    : IRequestHandler<AddBoardMemberCommand, AddBoardMemberResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;

    public AddBoardMemberHandler(IBoardRepository boards, IUnitOfWork uow)
    {
        _boards = boards;
        _uow = uow;
    }

    public async Task<AddBoardMemberResult> Handle(AddBoardMemberCommand cmd, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsync(cmd.BoardId, ct);
        if (board is null)
            throw new BoardNotFoundException(cmd.BoardId);

        var now = DateTimeOffset.UtcNow;

        board.AddMember(cmd.UserId, cmd.Role, now);

        await _uow.SaveChangesAsync(ct);

        return new AddBoardMemberResult(
            BoardId: cmd.BoardId,
            UserId: cmd.UserId,
            Role: cmd.Role);
    }
}
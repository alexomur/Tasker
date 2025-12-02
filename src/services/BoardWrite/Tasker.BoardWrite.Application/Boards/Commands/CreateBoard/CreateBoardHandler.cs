using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;

/// <summary>
/// Обработчик команды создания доски.
/// </summary>
public sealed class CreateBoardHandler
    : IRequestHandler<CreateBoardCommand, CreateBoardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;

    public CreateBoardHandler(IBoardRepository boards, IUnitOfWork uow)
    {
        _boards = boards;
        _uow = uow;
    }

    public async Task<CreateBoardResult> Handle(CreateBoardCommand cmd, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var board = Board.Create(
            title: cmd.Title,
            ownerUserId: cmd.OwnerUserId,
            now: now,
            description: cmd.Description);

        await _boards.AddAsync(board, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreateBoardResult(board.Id);
    }
}
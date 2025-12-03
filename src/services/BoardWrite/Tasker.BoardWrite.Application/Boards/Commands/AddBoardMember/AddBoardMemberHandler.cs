using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddBoardMember
{
    /// <summary>
    /// Обработчик команды добавления участника.
    /// </summary>
    public sealed class AddBoardMemberHandler
        : IRequestHandler<AddBoardMemberCommand, AddBoardMemberResult>
    {
        private readonly IBoardRepository _boards;
        private readonly IUnitOfWork _uow;
        private readonly IBoardAccessService _boardAccess;

        public AddBoardMemberHandler(
            IBoardRepository boards,
            IUnitOfWork uow,
            IBoardAccessService boardAccess)
        {
            _boards = boards;
            _uow = uow;
            _boardAccess = boardAccess;
        }

        public async Task<AddBoardMemberResult> Handle(AddBoardMemberCommand cmd, CancellationToken ct)
        {
            var board = await _boards.GetByIdAsTrackingAsync(cmd.BoardId, ct);
            if (board is null)
            {
                throw new BoardNotFoundException(cmd.BoardId);
            }

            await _boardAccess.EnsureCanManageMembersAsync(board.Id, ct);

            var now = DateTimeOffset.UtcNow;

            var member = board.AddMember(cmd.UserId, cmd.Role, now);

            await _boards.AddEntityAsync(member, ct);

            await _uow.SaveChangesAsync(ct);

            return new AddBoardMemberResult(
                BoardId: cmd.BoardId,
                UserId: cmd.UserId,
                Role: cmd.Role);
        }
    }
}
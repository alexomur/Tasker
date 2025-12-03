using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Security
{
    /// <summary>
    /// Реализация сервиса проверки прав доступа к доске на основе ролей участников.
    /// При необходимости легко заменить/расширить до ABAC.
    /// </summary>
    public sealed class BoardAccessService : IBoardAccessService
    {
        private readonly BoardWriteDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public BoardAccessService(
            BoardWriteDbContext dbContext,
            ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task EnsureCanReadBoardAsync(Guid boardId, CancellationToken ct)
        {
            var access = await GetAccessForCurrentUserAsync(boardId, ct);
            if (!access.CanRead)
            {
                throw new BoardAccessDeniedException(boardId, access.UserId);
            }
        }

        public async Task EnsureCanWriteBoardAsync(Guid boardId, CancellationToken ct)
        {
            var access = await GetAccessForCurrentUserAsync(boardId, ct);
            if (!access.CanWrite)
            {
                throw new BoardAccessDeniedException(boardId, access.UserId);
            }
        }

        public async Task EnsureCanManageMembersAsync(Guid boardId, CancellationToken ct)
        {
            var access = await GetAccessForCurrentUserAsync(boardId, ct);
            if (!access.CanManageMembers)
            {
                throw new BoardAccessDeniedException(boardId, access.UserId);
            }
        }

        private async Task<BoardAccess> GetAccessForCurrentUserAsync(Guid boardId, CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            {
                throw new BoardAccessDeniedException(boardId, Guid.Empty);
            }

            var userId = _currentUser.UserId.Value;

            var member = await _dbContext.BoardMembers
                .Where(m => m.BoardId == boardId && m.UserId == userId && m.LeftAt == null)
                .Select(m => new
                {
                    m.UserId,
                    m.Role
                })
                .SingleOrDefaultAsync(ct);

            if (member is null)
            {
                throw new BoardAccessDeniedException(boardId, userId);
            }

            var canRead = true;
            var canWrite = member.Role is BoardMemberRole.Owner
                or BoardMemberRole.Admin
                or BoardMemberRole.Member;
            var canManageMembers = member.Role is BoardMemberRole.Owner
                or BoardMemberRole.Admin;

            return new BoardAccess(userId, canRead, canWrite, canManageMembers);
        }

        private readonly struct BoardAccess
        {
            public BoardAccess(Guid userId, bool canRead, bool canWrite, bool canManageMembers)
            {
                UserId = userId;
                CanRead = canRead;
                CanWrite = canWrite;
                CanManageMembers = canManageMembers;
            }

            public Guid UserId { get; }

            public bool CanRead { get; }

            public bool CanWrite { get; }

            public bool CanManageMembers { get; }
        }
    }
}

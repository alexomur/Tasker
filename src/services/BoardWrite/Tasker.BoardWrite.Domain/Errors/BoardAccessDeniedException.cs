using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors
{
    /// <summary>
    /// Исключение, выбрасываемое при отсутствии прав доступа пользователя к доске.
    /// </summary>
    public sealed class BoardAccessDeniedException : AppException
    {
        /// <summary>
        /// Идентификатор доски, к которой пытались получить доступ.
        /// </summary>
        public Guid BoardId { get; }

        /// <summary>
        /// Идентификатор пользователя, для которого произошёл отказ в доступе.
        /// </summary>
        public Guid UserId { get; }

        public BoardAccessDeniedException(Guid boardId, Guid userId) : base($"User '{userId}' has no permission to write at board '{boardId}'.", "board_write.access_denied", 403)
        {
            BoardId = boardId;
            UserId = userId;
        }
    }
}
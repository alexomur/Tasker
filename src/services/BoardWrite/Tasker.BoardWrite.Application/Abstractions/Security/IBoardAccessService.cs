
namespace Tasker.BoardWrite.Application.Abstractions.Security
{
    /// <summary>
    /// Сервис проверки прав доступа текущего пользователя к доске.
    /// Инкапсулирует правила RBAC/ABAC, чтобы не привязываться напрямую к ролям.
    /// </summary>
    public interface IBoardAccessService
    {
        /// <summary>
        /// Гарантирует, что текущий пользователь может читать указанную доску.
        /// В случае отсутствия прав выбрасывает исключение.
        /// </summary>
        Task EnsureCanReadBoardAsync(Guid boardId, CancellationToken ct);

        /// <summary>
        /// Гарантирует, что текущий пользователь может изменять содержимое доски
        /// (колонки, карточки, метки).
        /// </summary>
        Task EnsureCanWriteBoardAsync(Guid boardId, CancellationToken ct);

        /// <summary>
        /// Гарантирует, что текущий пользователь может управлять участниками доски.
        /// </summary>
        Task EnsureCanManageMembersAsync(Guid boardId, CancellationToken ct);
    }
}
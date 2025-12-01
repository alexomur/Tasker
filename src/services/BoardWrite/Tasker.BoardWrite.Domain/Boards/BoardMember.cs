using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Участник доски, связывающий пользователя с конкретной доской и его ролью на ней.
/// </summary>
public sealed class BoardMember : Entity
{
    /// <summary>
    /// Идентификатор доски, к которой относится участник.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, являющегося участником доски.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Роль участника на доске, определяющая его права.
    /// </summary>
    public BoardMemberRole Role { get; private set; }

    /// <summary>
    /// Дата и время добавления участника на доску в формате UTC.
    /// </summary>
    public DateTimeOffset JoinedAt { get; private set; }

    /// <summary>
    /// Дата и время удаления участника с доски в формате UTC. Отсутствует, если участник активен.
    /// </summary>
    public DateTimeOffset? LeftAt { get; private set; }

    /// <summary>
    /// Признак того, что участник сейчас активен на доске (не удалён).
    /// </summary>
    public bool IsActive => LeftAt is null;

    protected BoardMember() { }

    /// <summary>
    /// Создаёт нового участника доски с указанной ролью.
    /// </summary>
    /// <param name="boardId">Идентификатор доски.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="role">Роль участника на доске.</param>
    /// <param name="joinedAt">Дата и время добавления на доску (UTC).</param>
    public BoardMember(Guid boardId, Guid userId, BoardMemberRole role, DateTimeOffset joinedAt)
    {
        if (boardId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор доски не может быть пустым.", nameof(boardId));
        }
        
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор пользователя не может быть пустым.", nameof(userId));
        }
        
        BoardId = boardId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    /// <summary>
    /// Изменяет роль участника на доске.
    /// </summary>
    /// <param name="role">Новая роль участника.</param>
    public void ChangeRole(BoardMemberRole role)
    {
        Role = role;
    }

    /// <summary>
    /// Помечает участника как удалённого с доски.
    /// </summary>
    /// <param name="leftAt">Дата и время удаления участника (UTC).</param>
    public void Leave(DateTimeOffset leftAt)
    {
        if (!IsActive)
        {
            return;
        }

        LeftAt = leftAt;
    }
}
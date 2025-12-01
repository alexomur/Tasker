namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Роль участника доски, определяющая уровень его доступа.
/// </summary>
public enum BoardMemberRole
{
    /// <summary>
    /// Владелец доски с полным набором прав.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Администратор доски с расширенными правами управления.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Обычный участник доски с базовыми правами работы с карточками.
    /// </summary>
    Member = 2,

    /// <summary>
    /// Наблюдатель с правами только на чтение.
    /// </summary>
    Viewer = 3
}
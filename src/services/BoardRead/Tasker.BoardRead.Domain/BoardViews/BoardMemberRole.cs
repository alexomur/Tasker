namespace Tasker.BoardRead.Domain.BoardViews;

// TODO: Move all authorization away from here
/// <summary>
/// Роль участника доски для read-слоя.
/// Значения совпадают с Tasker.BoardWrite.Domain.Boards.BoardMemberRole.
/// </summary>
public enum BoardMemberRole
{
    Owner = 0,
    Admin = 1,
    Member = 2,
    Viewer = 3
}
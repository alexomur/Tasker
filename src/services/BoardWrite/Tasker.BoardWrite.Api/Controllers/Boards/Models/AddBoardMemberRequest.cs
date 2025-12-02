using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на добавление участника доски.
/// </summary>
public sealed class AddBoardMemberRequest
{
    public Guid UserId { get; set; }
    public BoardMemberRole Role { get; set; }
}
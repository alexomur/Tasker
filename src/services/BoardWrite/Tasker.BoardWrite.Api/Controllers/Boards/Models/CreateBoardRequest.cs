namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на создание доски.
/// </summary>
public sealed class CreateBoardRequest
{
    public string Title { get; set; } = null!;
    public Guid OwnerUserId { get; set; }
    public string? Description { get; set; }
}
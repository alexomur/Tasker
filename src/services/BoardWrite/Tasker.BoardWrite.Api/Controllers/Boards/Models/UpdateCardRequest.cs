namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

public class UpdateCardRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
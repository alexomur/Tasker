namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на добавление метки.
/// </summary>
public sealed class AddLabelRequest
{
    public string Title { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? Description { get; set; }
}
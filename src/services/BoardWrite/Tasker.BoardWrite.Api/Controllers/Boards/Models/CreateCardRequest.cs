namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на создание карточки в колонке.
/// </summary>
public sealed class CreateCardRequest
{
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на добавление колонки.
/// </summary>
public sealed class AddColumnRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Тело запроса для создания доски.
/// </summary>
public sealed record CreateBoardRequest(
    string Title,
    string? Description,
    string? TemplateCode
);
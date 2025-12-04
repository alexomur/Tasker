namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на перемещение карточки в другую колонку.
/// </summary>
public sealed class MoveCardRequest
{
    /// <summary>
    /// Идентификатор колонки, в которую нужно переместить карточку.
    /// </summary>
    public Guid TargetColumnId { get; set; }
}
namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

/// <summary>
/// Запрос на установку или сброс дедлайна карточки.
/// </summary>
public sealed class SetCardDueDateRequest
{
    /// <summary>
    /// Новая дата дедлайна в UTC. Null — сбросить дедлайн.
    /// </summary>
    public DateTimeOffset? DueDate { get; set; }
}
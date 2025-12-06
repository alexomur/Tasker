using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Abstractions.Services;

/// <summary>
/// Сервис применения шаблонов к только что созданной доске.
/// Шаблон заполняет колонки/метки/базовый workflow.
/// </summary>
public interface IBoardTemplateService
{
    /// <summary>
    /// Применить шаблон к доске.
    /// Если templateCode == null или пустой, шаблон не применяется.
    /// Не должен ломать инварианты и может спокойно вызываться только для "свежесозданных" досок.
    /// </summary>
    void ApplyTemplate(Board board, string? templateCode, Guid ownerUserId, DateTimeOffset now);
}
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Application.Boards.Templates;

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
    /// </summary>
    void ApplyTemplate(Board board, string? templateCode, Guid ownerUserId, DateTimeOffset now);

    /// <summary>
    /// Вернуть список доступных шаблонов досок.
    /// Используется для фронта и документации.
    /// </summary>
    IReadOnlyCollection<BoardTemplateInfo> GetTemplates();
}
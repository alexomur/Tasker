using Tasker.BoardWrite.Application.Abstractions.Services;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Boards.Templates;

/// <summary>
/// Базовая реализация шаблонов.
/// Пока без внешних зависимостей — чистая логика поверх агрегата Board.
/// </summary>
public sealed class BoardTemplateService : IBoardTemplateService
{
    public void ApplyTemplate(Board board, string? templateCode, Guid ownerUserId, DateTimeOffset now)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));

        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return;
        }

        if (board.Columns.Any() || board.Labels.Any())
        {
            return;
        }

        switch (templateCode)
        {
            case BoardTemplateCodes.SoftwareKanban:
                ApplySoftwareKanban(board, now);
                break;

            case BoardTemplateCodes.GameDevFeature:
                ApplyGameDevFeaturePipeline(board, now);
                break;

            case BoardTemplateCodes.GameDevContent:
                ApplyGameDevContentPipeline(board, now);
                break;

            default:
                break;
        }
    }

    private static void ApplySoftwareKanban(Board board, DateTimeOffset now)
    {
        board.AddColumn("Backlog", now, "Идеи и задачи без приоритета");
        board.AddColumn("To Do", now, "Отобранные задачи к работе");
        board.AddColumn("In Progress", now, "В работе");
        board.AddColumn("In Review", now, "Код-ревью / ревью задачи");
        board.AddColumn("Done", now, "Готово");

        board.AddLabel("Bug", "#d32f2f", "Баг/дефект");
        board.AddLabel("Feature", "#1976d2", "Новая функциональность");
        board.AddLabel("Tech Debt", "#5d4037", "Технический долг");
        board.AddLabel("Research", "#7b1fa2", "Исследование / прототип");
    }

    /// <summary>
    /// Геймдев-шаблон для фич: от идеи до релиза.
    /// Подходит для геймдизайнеров, программистов, аналитиков.
    /// </summary>
    private static void ApplyGameDevFeaturePipeline(Board board, DateTimeOffset now)
    {
        board.AddColumn("Ideas", now, "Сырой пул идей, питчей и хотелок");
        board.AddColumn("Design in progress", now, "Геймдизайн прорабатывает механику");
        board.AddColumn("Design ready", now, "Диздок и спецификация утверждены");
        board.AddColumn("Implementation", now, "Реализация фичи (код, скрипты)");
        board.AddColumn("Playtest", now, "Игровое тестирование и баланс");
        board.AddColumn("Ready for release", now, "Готово к выкатку в билд");

        board.AddLabel("Core mechanic", "#1976d2", "Ключевая игровая механика");
        board.AddLabel("Content", "#388e3c", "Контент (уровни, квесты, лут)");
        board.AddLabel("UI/UX", "#fbc02d", "Интерфейс и UX");
        board.AddLabel("Balancing", "#7b1fa2", "Баланс, числа, экономики");
        board.AddLabel("Tech", "#455a64", "Техническая задача, поддержка движка");
    }

    /// <summary>
    /// Геймдев-шаблон для контента / арта.
    /// Подходит для художников, моделеров, аниматоров и левел-дизайнеров.
    /// </summary>
    private static void ApplyGameDevContentPipeline(Board board, DateTimeOffset now)
    {
        board.AddColumn("Concept", now, "Концепты, референсы, мудборды");
        board.AddColumn("Blockout / Prototype", now, "Блок-аут / грубый прототип");
        board.AddColumn("Production", now, "Производство: моделлинг, текстуры, анимации");
        board.AddColumn("Integration", now, "Интеграция в игру / движок");
        board.AddColumn("QA / Balancing", now, "Проверка, правки, оптимизация");
        board.AddColumn("Released", now, "Вошло в игру / контент-апдейт");

        board.AddLabel("Environment", "#388e3c", "Окружение / уровни");
        board.AddLabel("Character", "#c2185b", "Персонажи");
        board.AddLabel("VFX", "#7c4dff", "Эффекты");
        board.AddLabel("UI Art", "#ffa000", "Арты для UI");
        board.AddLabel("Optimization", "#455a64", "Оптимизация контента");
    }
}

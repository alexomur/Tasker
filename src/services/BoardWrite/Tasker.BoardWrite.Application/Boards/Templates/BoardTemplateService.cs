using Tasker.BoardWrite.Application.Abstractions.Services;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Boards.Templates;

/// <summary>
/// Базовая реализация шаблонов досок.
/// Держит и описательную инфу (для фронта), и фактическое наполнение досок.
/// </summary>
public sealed class BoardTemplateService : IBoardTemplateService
{
    private static readonly IReadOnlyList<BoardTemplateInfo> Templates = new[]
    {
        new BoardTemplateInfo(
            Code: BoardTemplateCodes.SoftwareKanban,
            Name: "Канбан для разработки",
            Description: "Простой канбан для продуктовой/тех команды: Backlog → To Do → In Progress → In Review → Done.",
            Category: "software",
            Tags: new[] { "kanban", "software", "dev" }),

        new BoardTemplateInfo(
            Code: BoardTemplateCodes.GameDevFeature,
            Name: "Геймдев: фичи",
            Description: "Пайплайн для игровых фич: идеи → геймдизайн → реализация → плейтест → релиз.",
            Category: "gamedev",
            Tags: new[] { "gamedev", "feature", "design", "programming" }),

        new BoardTemplateInfo(
            Code: BoardTemplateCodes.GameDevContent,
            Name: "Геймдев: контент/арт",
            Description: "Пайплайн для арта/контента: концепт → блок-аут → продакшен → интеграция → QA → релиз.",
            Category: "gamedev",
            Tags: new[] { "gamedev", "art", "content" })
    };

    public IReadOnlyCollection<BoardTemplateInfo> GetTemplates() => Templates;

    public void ApplyTemplate(Board board, string? templateCode, Guid ownerUserId, DateTimeOffset now)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));

        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return;
        }

        // Защита: не лезем в уже живую доску
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
        }
    }

    private static void ApplySoftwareKanban(Board board, DateTimeOffset now)
    {
        board.AddColumn("Backlog", now, "Неотсортированные задачи и идеи");
        board.AddColumn("To Do", now, "Отобранные задачи к работе");
        board.AddColumn("In Progress", now, "В работе");
        board.AddColumn("In Review", now, "Код-ревью / ревью задачи");
        board.AddColumn("Done", now, "Готово");

        board.AddLabel("Bug", "#d32f2f", "Баг/дефект");
        board.AddLabel("Feature", "#1976d2", "Новая фича");
        board.AddLabel("Tech Debt", "#5d4037", "Технический долг");
        board.AddLabel("Research", "#7b1fa2", "Исследование / прототип");
    }

    /// <summary>
    /// Геймдев-шаблон для фич: от идеи до релиза.
    /// Для геймдизайнеров, программистов, аналитиков.
    /// </summary>
    private static void ApplyGameDevFeaturePipeline(Board board, DateTimeOffset now)
    {
        board.AddColumn("Ideas", now, "Сырой пул идей и питчей");
        board.AddColumn("Design in progress", now, "Геймдизайнер прорабатывает механику");
        board.AddColumn("Design ready", now, "Диздок/спека утверждены");
        board.AddColumn("Implementation", now, "Реализация (код, скрипты, настройки)");
        board.AddColumn("Playtest", now, "Внутренний плейтест и баланс");
        board.AddColumn("Ready for release", now, "Готово к включению в релизный билд");

        board.AddLabel("Core mechanic", "#1976d2", "Ключевая игровая механика");
        board.AddLabel("Content", "#388e3c", "Контент (квесты, уровни, лут)");
        board.AddLabel("UI/UX", "#fbc02d", "Интерфейс и UX");
        board.AddLabel("Balancing", "#7b1fa2", "Баланс / экономика");
        board.AddLabel("Tech", "#455a64", "Техническая задача под фичу");
    }

    /// <summary>
    /// Геймдев-шаблон для контента / арта.
    /// Для художников, моделеров, аниматоров, левел-дизайнеров.
    /// </summary>
    private static void ApplyGameDevContentPipeline(Board board, DateTimeOffset now)
    {
        board.AddColumn("Concept", now, "Концепты, референсы, мудборды");
        board.AddColumn("Blockout / Prototype", now, "Грубый блок-аут / прототип");
        board.AddColumn("Production", now, "Моделлинг, текстуры, анимации и т.п.");
        board.AddColumn("Integration", now, "Интеграция ассетов в движок/сцену");
        board.AddColumn("QA / Polishing", now, "Проверка, правки, оптимизация");
        board.AddColumn("Released", now, "Контент в shipped билде");

        board.AddLabel("Environment", "#388e3c", "Окружение / уровни");
        board.AddLabel("Character", "#c2185b", "Персонажи");
        board.AddLabel("VFX", "#7c4dff", "Эффекты");
        board.AddLabel("UI Art", "#ffa000", "UI-арт");
        board.AddLabel("Optimization", "#455a64", "Оптимизация контента");
    }
}

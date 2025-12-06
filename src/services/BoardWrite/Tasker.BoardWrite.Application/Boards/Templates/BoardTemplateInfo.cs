namespace Tasker.BoardWrite.Application.Boards.Templates;

/// <summary>
/// Описательная информация о шаблоне доски, чтобы фронт мог красиво её показать.
/// </summary>
public sealed record BoardTemplateInfo(
    string Code,
    string Name,
    string Description,
    string Category,
    IReadOnlyCollection<string> Tags
);
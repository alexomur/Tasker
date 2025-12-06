namespace Tasker.BoardWrite.Application.Boards.Templates;

/// <summary>
/// Строковые коды шаблонов досок.
/// Будут использоваться и на бэкенде, и на фронте.
/// </summary>
public static class BoardTemplateCodes
{
    /// <summary>
    /// Обычный канбан для софта.
    /// </summary>
    public const string SoftwareKanban = "default/software";

    /// <summary>
    /// Геймдев: фича-пайплайн (от идеи до релиза).
    /// </summary>
    public const string GameDevFeature = "gamedev/feature";

    /// <summary>
    /// Геймдев: контент-пайплайн (арт/контент).
    /// </summary>
    public const string GameDevContent = "gamedev/content";
}
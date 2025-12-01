using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Колонка на доске, объединяющая карточки в рамках одного этапа процесса.
/// </summary>
public sealed class Column : Entity
{
    /// <summary>
    /// Идентификатор доски, к которой принадлежит колонка.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// Название колонки, отображаемое на доске.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Дополнительное описание колонки. Может отсутствовать.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Порядок колонки на доске, используемый для сортировки слева направо.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Дата и время создания колонки в формате UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения колонки в формате UTC.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    protected Column() { }

    private Column(
        Guid boardId,
        string title,
        int order,
        DateTimeOffset now,
        string? description)
    {
        if (boardId == Guid.Empty)
            throw new ArgumentException("Идентификатор доски не может быть пустым.", nameof(boardId));

        SetTitle(title);
        SetDescription(description);

        BoardId = boardId;
        Order = order;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Создаёт новую колонку на указанной доске.
    /// </summary>
    /// <param name="boardId">Идентификатор доски.</param>
    /// <param name="title">Название колонки.</param>
    /// <param name="order">Порядок колонки на доске.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    /// <param name="description">Описание колонки, может быть пустым или null.</param>
    public static Column Create(
        Guid boardId,
        string title,
        int order,
        DateTimeOffset now,
        string? description = null)
    {
        return new Column(boardId, title, order, now, description);
    }

    /// <summary>
    /// Переименовывает колонку.
    /// </summary>
    /// <param name="title">Новое название колонки.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void Rename(string title, DateTimeOffset now)
    {
        SetTitle(title);
        Touch(now);
    }

    /// <summary>
    /// Изменяет описание колонки.
    /// </summary>
    /// <param name="description">Новое описание колонки, может быть пустым или null.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void ChangeDescription(string? description, DateTimeOffset now)
    {
        SetDescription(description);
        Touch(now);
    }

    /// <summary>
    /// Изменяет порядок колонки на доске.
    /// </summary>
    /// <param name="order">Новый порядок колонки.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void Reorder(int order, DateTimeOffset now)
    {
        Order = order;
        Touch(now);
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название колонки не может быть пустым.", nameof(title));

        Title = title.Trim();
    }

    private void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
}

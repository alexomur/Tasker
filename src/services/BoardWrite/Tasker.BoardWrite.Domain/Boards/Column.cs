using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Колонка на доске, содержит упорядоченный набор карточек.
/// </summary>
public sealed class Column : Entity
{
    /// <summary>
    /// Идентификатор доски, к которой принадлежит колонка.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// Название колонки.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Дополнительное описание колонки.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Порядок колонки на доске (0,1,2,...).
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Дата создания (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Дата последнего изменения (UTC).
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    // Для EF
    private Column() { }

    private Column(
        Guid boardId,
        string title,
        int order,
        DateTimeOffset now,
        string? description)
    {
        if (boardId == Guid.Empty)
            throw new ArgumentException("Идентификатор доски не может быть пустым.", nameof(boardId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название колонки не может быть пустым.", nameof(title));

        BoardId = boardId;
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Order = order;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Создаёт новую колонку.
    /// </summary>
    public static Column Create(
        Guid boardId,
        string title,
        int order,
        DateTimeOffset now,
        string? description = null)
        => new Column(boardId, title, order, now, description);

    /// <summary>
    /// Переименовать колонку.
    /// </summary>
    public void Rename(string title, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название колонки не может быть пустым.", nameof(title));

        Title = title.Trim();
        Touch(now);
    }

    /// <summary>
    /// Изменить описание.
    /// </summary>
    public void ChangeDescription(string? description, DateTimeOffset now)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch(now);
    }

    /// <summary>
    /// Изменить порядок колонки.
    /// </summary>
    public void Reorder(int newOrder, DateTimeOffset now)
    {
        if (newOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(newOrder), "Порядок колонки не может быть отрицательным.");

        Order = newOrder;
        Touch(now);
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
}

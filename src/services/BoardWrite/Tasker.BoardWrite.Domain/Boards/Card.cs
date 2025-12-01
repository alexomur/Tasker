using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

public sealed class Card : Entity
{
    private readonly List<Label> _labels = new();
    private readonly List<Guid> _assigneeUserIds = new();

    /// <summary>
    /// Заголовок карточки, краткое описание задачи.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Подробное текстовое описание задачи.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Набор меток, используемых для классификации и фильтрации карточки.
    /// </summary>
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    /// <summary>
    /// Идентификатор колонки, в которой сейчас находится карточка.
    /// </summary>
    public Guid ColumnId { get; private set; }

    /// <summary>
    /// Порядок карточки внутри колонки, используется для сортировки на доске.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Дата и время создания карточки в формате UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения карточки в формате UTC.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего карточку.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Идентификаторы пользователей, назначенных исполнителями по данной карточке.
    /// </summary>
    public IReadOnlyCollection<Guid> AssigneeUserIds => _assigneeUserIds.AsReadOnly();

    /// <summary>
    /// Дата и время, к которому задача должна быть выполнена. Отсутствует, если дедлайн не задан.
    /// </summary>
    public DateTimeOffset? DueDate { get; private set; }

    private Card() { }

    private Card(
        Guid columnId,
        string title,
        Guid createdByUserId,
        int order,
        DateTimeOffset now,
        string? description = null,
        DateTimeOffset? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        ColumnId = columnId;
        Title = title.Trim();
        Description = description;
        CreatedByUserId = createdByUserId;
        Order = order;
        CreatedAt = now;
        UpdatedAt = now;
        DueDate = dueDate;
    }

    public static Card Create(
        Guid columnId,
        string title,
        Guid createdByUserId,
        int order,
        DateTimeOffset now,
        string? description = null,
        DateTimeOffset? dueDate = null)
    {
        var card = new Card(columnId, title, createdByUserId, order, now, description, dueDate);

        return card;
    }

    public void Rename(string title, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        Title = title.Trim();
        Touch(now);
    }

    public void ChangeDescription(string? description, DateTimeOffset now)
    {
        Description = description;
        Touch(now);
    }

    public void MoveToColumn(Guid newColumnId, int newOrder, DateTimeOffset now)
    {
        ColumnId = newColumnId;
        Order = newOrder;
        Touch(now);
    }

    public void Reorder(int newOrder, DateTimeOffset now)
    {
        Order = newOrder;
        Touch(now);
    }

    public void SetDueDate(DateTimeOffset? dueDate, DateTimeOffset now)
    {
        DueDate = dueDate;
        Touch(now);
    }

    public void AssignUser(Guid userId, DateTimeOffset now)
    {
        if (!_assigneeUserIds.Contains(userId))
        {
            _assigneeUserIds.Add(userId);
            Touch(now);
        }
    }

    public void UnassignUser(Guid userId, DateTimeOffset now)
    {
        if (_assigneeUserIds.Remove(userId))
        {
            Touch(now);
        }
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
}

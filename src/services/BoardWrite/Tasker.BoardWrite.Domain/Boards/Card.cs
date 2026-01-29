using Tasker.BoardWrite.Domain.Events.CardEvents;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Карточка задачи на доске. Хранится в рамках конкретной доски и относится к одной колонке.
/// </summary>
public sealed class Card : Entity
{
    private readonly List<Label> _labels = new();
    private readonly List<Guid> _assigneeUserIds = new();

    /// <summary>
    /// Идентификатор доски, к которой принадлежит карточка.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// Идентификатор колонки, в которой сейчас находится карточка.
    /// </summary>
    public Guid ColumnId { get; private set; }

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
        Guid boardId,
        Guid columnId,
        string title,
        Guid createdByUserId,
        int order,
        DateTimeOffset now,
        string? description = null,
        DateTimeOffset? dueDate = null)
    {
        if (boardId == Guid.Empty)
            throw new ArgumentException("BoardId cannot be empty.", nameof(boardId));

        if (columnId == Guid.Empty)
            throw new ArgumentException("ColumnId cannot be empty.", nameof(columnId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        BoardId = boardId;
        ColumnId = columnId;
        Title = title.Trim();
        Description = NormalizeDescription(description);
        CreatedByUserId = createdByUserId;
        Order = order;
        CreatedAt = now;
        UpdatedAt = now;
        DueDate = dueDate;
    }

    /// <summary>
    /// Создаёт новую карточку для указанной доски и колонки.
    /// </summary>
    public static Card Create(
        Guid boardId,
        Guid columnId,
        string title,
        Guid createdByUserId,
        int order,
        DateTimeOffset now,
        string? description = null,
        DateTimeOffset? dueDate = null)
    {
        var card = new Card(boardId, columnId, title, createdByUserId, order, now, description, dueDate);
        card.AddEvent(new CardCreated(
            BoardId: card.BoardId,
            CardId: card.Id,
            ColumnId: card.ColumnId,
            Title: card.Title,
            Description: card.Description,
            Order: card.Order,
            CreatedByUserId: createdByUserId,
            OccurredAt: now));
        return card;
    }

    /// <summary>
    /// Переименовывает карточку.
    /// </summary>
    public void Rename(string title, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title.Trim();
        Touch(now);
    }

    /// <summary>
    /// Изменяет описание карточки.
    /// </summary>
    public void ChangeDescription(string? description, DateTimeOffset now)
    {
        Description = NormalizeDescription(description);
        Touch(now);
    }

    public void UpdateDetails(string title, string? description, Guid updatedByUserId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var normalizedTitle = title.Trim();
        var normalizedDescription = NormalizeDescription(description);

        if (string.Equals(Title, normalizedTitle, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal))
        {
            return;
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        Touch(now);

        AddEvent(new CardUpdated(
            BoardId: BoardId,
            CardId: Id,
            Title: Title,
            Description: Description,
            UpdatedByUserId: updatedByUserId,
            OccurredAt: now));
    }

    /// <summary>
    /// Перемещает карточку в другую колонку с указанным порядком.
    /// </summary>
    public void MoveToColumn(Guid newColumnId, int newOrder, Guid movedByUserId, DateTimeOffset now)
    {
        if (newColumnId == Guid.Empty)
            throw new ArgumentException("ColumnId cannot be empty.", nameof(newColumnId));

        var fromColumnId = ColumnId;
        ColumnId = newColumnId;
        Order = newOrder;
        Touch(now);

        AddEvent(new CardMoved(
            BoardId: BoardId,
            CardId: Id,
            FromColumnId: fromColumnId,
            ToColumnId: ColumnId,
            Order: Order,
            MovedByUserId: movedByUserId,
            OccurredAt: now));
    }

    /// <summary>
    /// Изменяет порядок карточки внутри текущей колонки.
    /// </summary>
    public void Reorder(int newOrder, DateTimeOffset now)
    {
        Order = newOrder;
        Touch(now);
    }

    public void SetDueDate(DateTimeOffset? dueDate, Guid changedByUserId, DateTimeOffset now)
    {
        DueDate = dueDate;
        Touch(now);

        AddEvent(new CardDueDateChanged(
            BoardId: BoardId,
            CardId: Id,
            NewDueDate: DueDate,
            ChangedByUserId: changedByUserId,
            OccurredAt: now));
    }

    public void AssignUser(Guid userId, Guid changedByUserId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (_assigneeUserIds.Contains(userId))
            return;

        _assigneeUserIds.Add(userId);
        Touch(now);

        AddEvent(new CardAssigneesChanged(
            BoardId: BoardId,
            CardId: Id,
            AssigneeUserIds: _assigneeUserIds.ToArray(),
            ChangedByUserId: changedByUserId,
            OccurredAt: now));
    }

    public void UnassignUser(Guid userId, Guid changedByUserId, DateTimeOffset now)
    {
        if (!_assigneeUserIds.Remove(userId))
            return;

        Touch(now);

        AddEvent(new CardAssigneesChanged(
            BoardId: BoardId,
            CardId: Id,
            AssigneeUserIds: _assigneeUserIds.ToArray(),
            ChangedByUserId: changedByUserId,
            OccurredAt: now));
    }

    public void AddLabel(Label label, Guid attachedByUserId, DateTimeOffset now)
    {
        if (label is null)
            throw new ArgumentNullException(nameof(label));

        if (_labels.Any(l => l.Id == label.Id))
            return;

        _labels.Add(label);
        Touch(now);

        AddEvent(new CardLabelAttached(
            BoardId: BoardId,
            CardId: Id,
            LabelId: label.Id,
            AttachedByUserId: attachedByUserId,
            OccurredAt: now));
    }

    public void RemoveLabel(Guid labelId, Guid detachedByUserId, DateTimeOffset now)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label is null)
            return;

        _labels.Remove(label);
        Touch(now);

        AddEvent(new CardLabelDetached(
            BoardId: BoardId,
            CardId: Id,
            LabelId: labelId,
            DetachedByUserId: detachedByUserId,
            OccurredAt: now));
    }

    public void MarkDeleted(Guid deletedByUserId, DateTimeOffset now)
    {
        AddEvent(new CardDeleted(
            BoardId: BoardId,
            CardId: Id,
            DeletedByUserId: deletedByUserId,
            OccurredAt: now));
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
    
    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}

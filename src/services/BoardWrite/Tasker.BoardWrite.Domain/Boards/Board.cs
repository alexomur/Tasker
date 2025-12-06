using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Доска с колонками, участниками, метками и карточками, представляющая один рабочий процесс.
/// </summary>
public sealed class Board : Entity
{
    private readonly List<Column> _columns = new();
    private readonly List<BoardMember> _members = new();
    private readonly List<Label> _labels = new();
    private readonly List<Card> _cards = new();

    /// <summary>
    /// Название доски, отображаемое пользователям.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Дополнительное текстовое описание доски. Может отсутствовать.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего доску и являющегося её владельцем.
    /// </summary>
    public Guid OwnerUserId { get; private set; }

    /// <summary>
    /// Признак того, что доска находится в архиве и недоступна для обычной работы.
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Дата и время создания доски в формате UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения доски в формате UTC.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Коллекция колонок, принадлежащих данной доске.
    /// </summary>
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();

    /// <summary>
    /// Коллекция участников, имеющих доступ к доске.
    /// </summary>
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Коллекция меток, доступных для карточек на этой доске.
    /// </summary>
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    /// <summary>
    /// Коллекция всех карточек данной доски. Позиционирование по колонкам определяется свойством ColumnId у карточки.
    /// </summary>
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    protected Board() { }

    private Board(
        string title,
        Guid ownerUserId,
        DateTimeOffset now,
        string? description)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Идентификатор владельца не может быть пустым.", nameof(ownerUserId));

        SetTitle(title);
        SetDescription(description);

        OwnerUserId = ownerUserId;
        CreatedAt = now;
        UpdatedAt = now;
        IsArchived = false;
    }

    /// <summary>
    /// Создаёт новую доску с указанным владельцем.
    /// </summary>
    /// <param name="title">Название доски.</param>
    /// <param name="ownerUserId">Идентификатор пользователя-владельца доски.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    /// <param name="description">Описание доски, может быть пустым или null.</param>
    public static Board Create(
        string title,
        Guid ownerUserId,
        DateTimeOffset now,
        string? description = null)
    {
        var board = new Board(title, ownerUserId, now, description);

        var ownerMember = new BoardMember(
            boardId: board.Id,
            userId: ownerUserId,
            role: BoardMemberRole.Owner,
            joinedAt: now);

        board._members.Add(ownerMember);

        return board;
    }

    /// <summary>
    /// Переименовывает доску.
    /// </summary>
    /// <param name="title">Новое название доски.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void Rename(string title, DateTimeOffset now)
    {
        SetTitle(title);
        Touch(now);
    }

    /// <summary>
    /// Изменяет описание доски.
    /// </summary>
    /// <param name="description">Новое описание доски, может быть пустым или null.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void ChangeDescription(string? description, DateTimeOffset now)
    {
        SetDescription(description);
        Touch(now);
    }

    /// <summary>
    /// Помещает доску в архив.
    /// </summary>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void Archive(DateTimeOffset now)
    {
        if (IsArchived)
            return;

        IsArchived = true;
        Touch(now);
    }

    /// <summary>
    /// Возвращает доску из архива.
    /// </summary>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void Restore(DateTimeOffset now)
    {
        if (!IsArchived)
            return;

        IsArchived = false;
        Touch(now);
    }

    /// <summary>
    /// Добавляет нового участника на доску.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="role">Роль участника на доске.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public BoardMember AddMember(Guid userId, BoardMemberRole role, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Идентификатор пользователя не может быть пустым.", nameof(userId));

        if (_members.Any(m => m.UserId == userId && m.IsActive))
            throw new InvalidOperationException("Пользователь уже является участником доски.");

        var member = new BoardMember(boardId: Id, userId, role, now);
        _members.Add(member);
        Touch(now);

        return member;
    }

    /// <summary>
    /// Удаляет участника с доски.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void RemoveMember(Guid userId, DateTimeOffset now)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member is null)
            return;

        if (member.Role == BoardMemberRole.Owner)
            throw new InvalidOperationException("Нельзя удалить владельца доски.");

        member.Leave(now);
        Touch(now);
    }

    /// <summary>
    /// Создаёт новую колонку на доске.
    /// </summary>
    /// <param name="title">Название колонки.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    /// <param name="description">Описание колонки, может быть пустым или null.</param>
    public Column AddColumn(string title, DateTimeOffset now, string? description = null)
    {
        var nextOrder = _columns.Count == 0
            ? 0
            : _columns.Max(c => c.Order) + 1;

        var column = Column.Create(Id, title, nextOrder, now, description);
        _columns.Add(column);
        Touch(now);

        return column;
    }

    /// <summary>
    /// Переставляет колонку на доске, изменяя её порядок.
    /// </summary>
    /// <param name="columnId">Идентификатор колонки.</param>
    /// <param name="newOrder">Новый порядок колонки.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void ReorderColumn(Guid columnId, int newOrder, DateTimeOffset now)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            throw new InvalidOperationException("Колонка не найдена на доске.");

        column.Reorder(newOrder, now);
        Touch(now);
    }

    /// <summary>
    /// Создаёт новую метку, доступную на доске.
    /// </summary>
    /// <param name="title">Название метки.</param>
    /// <param name="color">Цвет метки.</param>
    /// <param name="description">Описание метки, может быть пустым или null.</param>
    public Label AddLabel(string title, string color, string? description = null)
    {
        var label = new Label(title, description, color);
        _labels.Add(label);
        return label;
    }

    /// <summary>
    /// Создаёт новую карточку в указанной колонке данной доски.
    /// </summary>
    /// <param name="columnId">Идентификатор колонки, в которую добавляется карточка.</param>
    /// <param name="title">Заголовок карточки.</param>
    /// <param name="createdByUserId">Идентификатор пользователя, создавшего карточку.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    /// <param name="description">Подробное описание карточки, может быть пустым или null.</param>
    /// <param name="dueDate">Дедлайн, может отсутствовать.</param>
    public Card CreateCard(
        Guid columnId,
        string title,
        Guid createdByUserId,
        DateTimeOffset now,
        string? description = null,
        DateTimeOffset? dueDate = null)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            throw new InvalidOperationException("Колонка не найдена на доске.");

        var nextOrder = _cards
            .Where(c => c.ColumnId == columnId)
            .Select(c => c.Order)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var card = Card.Create(
            boardId: Id,
            columnId: columnId,
            title: title,
            createdByUserId: createdByUserId,
            order: nextOrder,
            now: now,
            description: description,
            dueDate: dueDate);

        _cards.Add(card);
        Touch(now);

        return card;
    }

    /// <summary>
    /// Перемещает карточку в другую колонку (или внутри той же колонки) с указанным порядком.
    /// </summary>
    /// <param name="cardId">Идентификатор карточки.</param>
    /// <param name="targetColumnId">Идентификатор целевой колонки.</param>
    /// <param name="targetOrder">
    /// Желаемая позиция карточки в целевой колонке. 
    /// Если не указано — карточка будет добавлена в конец.
    /// </param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void MoveCard(Guid cardId, Guid targetColumnId, int? targetOrder, DateTimeOffset now)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            throw new InvalidOperationException("Карточка не найдена на доске.");

        var targetColumn = _columns.FirstOrDefault(c => c.Id == targetColumnId);
        if (targetColumn is null)
            throw new InvalidOperationException("Целевая колонка не найдена на доске.");

        var order = targetOrder ?? _cards
            .Where(c => c.ColumnId == targetColumnId)
            .Select(c => c.Order)
            .DefaultIfEmpty(0)
            .Max() + 1;

        card.MoveToColumn(targetColumnId, order, now);
        Touch(now);
    }

    /// <summary>
    /// Изменяет порядок карточки внутри её колонки.
    /// </summary>
    /// <param name="cardId">Идентификатор карточки.</param>
    /// <param name="newOrder">Новый порядок карточки в колонке.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void ReorderCard(Guid cardId, int newOrder, DateTimeOffset now)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            throw new InvalidOperationException("Карточка не найдена на доске.");

        card.Reorder(newOrder, now);
        Touch(now);
    }

    /// <summary>
    /// Удаляет карточку с доски.
    /// </summary>
    /// <param name="cardId">Идентификатор карточки.</param>
    /// <param name="now">Текущее время в формате UTC.</param>
    public void RemoveCard(Guid cardId, DateTimeOffset now)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return;

        _cards.Remove(card);
        Touch(now);
    }

    public void AttachLabelToCard(Guid cardId, Guid labelId, DateTimeOffset now)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            throw new InvalidOperationException("Карточка не найдена на доске.");

        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label is null)
            throw new InvalidOperationException("Метка не найдена на доске.");

        card.AddLabel(label, now);
        Touch(now);
    }

    public void DetachLabelFromCard(Guid cardId, Guid labelId, DateTimeOffset now)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            throw new InvalidOperationException("Карточка не найдена на доске.");

        card.RemoveLabel(labelId, now);
        Touch(now);
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название доски не может быть пустым.", nameof(title));

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

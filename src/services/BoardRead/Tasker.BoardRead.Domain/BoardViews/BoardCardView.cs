namespace Tasker.BoardRead.Domain.BoardViews;

public sealed record BoardCardView(
    Guid Id,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DueDate,
    IReadOnlyCollection<Guid> AssigneeUserIds,
    IReadOnlyCollection<Guid> LabelIds);
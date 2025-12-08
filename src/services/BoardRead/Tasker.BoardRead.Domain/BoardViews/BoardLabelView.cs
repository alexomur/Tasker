namespace Tasker.BoardRead.Domain.BoardViews;

public sealed record BoardLabelView(
    Guid Id,
    string Title,
    string? Description,
    string Color);
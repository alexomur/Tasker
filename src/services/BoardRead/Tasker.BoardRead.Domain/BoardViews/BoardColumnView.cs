namespace Tasker.BoardRead.Domain.BoardViews;

public sealed record BoardColumnView(
    Guid Id,
    string Title,
    string? Description,
    int Order);
namespace Tasker.BoardRead.Domain.BoardViews;

/// <summary>
/// Краткое представление доски для списка «Мои доски».
/// Соответствует фронтовому BoardListItem.
/// </summary>
public sealed record BoardView(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    BoardMemberRole MyRole);
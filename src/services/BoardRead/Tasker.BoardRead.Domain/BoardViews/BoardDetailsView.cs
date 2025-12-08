using Tasker.BoardRead.Domain.UserViews;

namespace Tasker.BoardRead.Domain.BoardViews;

/// <summary>
/// Детальное представление доски для отображения всей доски.
/// По форме совместимо с BoardDetailsResult и фронтовым BoardDetails.
/// </summary>
public sealed record BoardDetailsView(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<BoardColumnView> Columns,
    IReadOnlyCollection<BoardMemberView> Members,
    IReadOnlyCollection<BoardLabelView> Labels,
    IReadOnlyCollection<BoardCardView> Cards,
    IReadOnlyCollection<UserView> Users);
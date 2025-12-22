namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a card has been created on a board.
/// </summary>
public sealed record BoardCardCreatedV1(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
);

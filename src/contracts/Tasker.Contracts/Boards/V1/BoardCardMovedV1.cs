namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a card has been moved.
/// </summary>
public sealed record BoardCardMovedV1(
    Guid BoardId,
    Guid CardId,
    Guid FromColumnId,
    Guid ToColumnId,
    int Order,
    Guid MovedByUserId,
    DateTimeOffset OccurredAt
);

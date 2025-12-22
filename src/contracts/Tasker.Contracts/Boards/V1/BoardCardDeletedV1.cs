namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a card has been deleted.
/// </summary>
public sealed record BoardCardDeletedV1(
    Guid BoardId,
    Guid CardId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
);

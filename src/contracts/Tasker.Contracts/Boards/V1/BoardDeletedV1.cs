namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a board has been deleted.
/// </summary>
public sealed record BoardDeletedV1(
    Guid BoardId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
);

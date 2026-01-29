namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a card has been updated.
/// </summary>
public sealed record BoardCardUpdatedV1(
    Guid BoardId,
    Guid CardId,
    string Title,
    string? Description,
    Guid UpdatedByUserId,
    DateTimeOffset OccurredAt
);

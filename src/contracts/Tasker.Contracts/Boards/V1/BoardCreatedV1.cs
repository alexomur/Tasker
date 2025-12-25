namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a board has been created.
/// </summary>
public sealed record BoardCreatedV1(
    Guid BoardId,
    Guid OwnerUserId,
    string Title,
    string? Description,
    DateTimeOffset OccurredAt
);

namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a board member has been removed.
/// </summary>
public sealed record BoardMemberRemovedV1(
    Guid BoardId,
    Guid UserId,
    Guid RemovedByUserId,
    DateTimeOffset OccurredAt
);

namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a board member has been added.
/// </summary>
public sealed record BoardMemberAddedV1(
    Guid BoardId,
    Guid UserId,
    int Role,
    Guid AddedByUserId,
    DateTimeOffset OccurredAt
);

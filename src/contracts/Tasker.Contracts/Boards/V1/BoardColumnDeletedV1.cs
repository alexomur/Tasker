namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a column has been deleted from a board.
/// </summary>
public sealed record BoardColumnDeletedV1(
    Guid BoardId,
    Guid ColumnId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
);

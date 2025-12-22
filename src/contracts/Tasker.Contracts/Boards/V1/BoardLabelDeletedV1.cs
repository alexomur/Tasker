namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a label has been deleted from a board.
/// </summary>
public sealed record BoardLabelDeletedV1(
    Guid BoardId,
    Guid LabelId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
);

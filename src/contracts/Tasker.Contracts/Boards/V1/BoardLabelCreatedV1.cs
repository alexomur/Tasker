namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a label has been created on a board.
/// </summary>
public sealed record BoardLabelCreatedV1(
    Guid BoardId,
    Guid LabelId,
    string Title,
    string? Description,
    string Color,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
);

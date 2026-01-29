namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a column has been created on a board.
/// </summary>
public sealed record BoardColumnCreatedV1(
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
);

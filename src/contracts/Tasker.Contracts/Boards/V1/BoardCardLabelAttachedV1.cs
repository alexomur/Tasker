namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a label has been attached to a card.
/// </summary>
public sealed record BoardCardLabelAttachedV1(
    Guid BoardId,
    Guid CardId,
    Guid LabelId,
    Guid AttachedByUserId,
    DateTimeOffset OccurredAt
);

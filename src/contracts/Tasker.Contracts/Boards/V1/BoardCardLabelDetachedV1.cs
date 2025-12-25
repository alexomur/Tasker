namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Integration event: a label has been detached from a card.
/// </summary>
public sealed record BoardCardLabelDetachedV1(
    Guid BoardId,
    Guid CardId,
    Guid LabelId,
    Guid DetachedByUserId,
    DateTimeOffset OccurredAt
);

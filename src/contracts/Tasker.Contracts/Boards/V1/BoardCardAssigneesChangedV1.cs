namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Интеграционное событие: изменился список исполнителей у карточки.
/// </summary>
public sealed record BoardCardAssigneesChangedV1(
    Guid BoardId,
    Guid CardId,
    IReadOnlyCollection<Guid> AssigneeUserIds,
    DateTimeOffset OccurredAt
);
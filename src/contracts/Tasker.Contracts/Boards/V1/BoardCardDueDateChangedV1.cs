namespace Tasker.Contracts.Boards.V1;

/// <summary>
/// Интеграционное событие: изменился дедлайн карточки.
/// </summary>
public sealed record BoardCardDueDateChangedV1(
    Guid BoardId,
    Guid CardId,
    DateTimeOffset? DueDate,
    Guid ChangedByUserId,
    DateTimeOffset OccurredAt
);

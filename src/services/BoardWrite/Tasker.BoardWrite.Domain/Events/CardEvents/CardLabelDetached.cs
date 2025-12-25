using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardLabelDetached(
    Guid BoardId,
    Guid CardId,
    Guid LabelId,
    Guid DetachedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

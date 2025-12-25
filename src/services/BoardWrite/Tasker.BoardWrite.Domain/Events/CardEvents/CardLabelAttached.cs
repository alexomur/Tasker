using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardLabelAttached(
    Guid BoardId,
    Guid CardId,
    Guid LabelId,
    Guid AttachedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

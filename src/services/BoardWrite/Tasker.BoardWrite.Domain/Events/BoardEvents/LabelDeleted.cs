using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record LabelDeleted(
    Guid BoardId,
    Guid LabelId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

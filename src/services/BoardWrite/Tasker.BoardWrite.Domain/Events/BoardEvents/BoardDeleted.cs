using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record BoardDeleted(
    Guid BoardId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

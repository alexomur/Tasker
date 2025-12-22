using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record BoardMemberRemoved(
    Guid BoardId,
    Guid UserId,
    Guid RemovedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

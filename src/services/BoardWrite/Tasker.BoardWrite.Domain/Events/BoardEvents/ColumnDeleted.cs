using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record ColumnDeleted(
    Guid BoardId,
    Guid ColumnId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

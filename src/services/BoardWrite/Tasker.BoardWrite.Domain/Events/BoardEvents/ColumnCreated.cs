using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record ColumnCreated(
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardMoved(
    Guid BoardId,
    Guid CardId,
    Guid FromColumnId,
    Guid ToColumnId,
    int Order,
    Guid MovedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

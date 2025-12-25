using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardDueDateChanged(
    Guid BoardId,
    Guid CardId,
    DateTimeOffset? NewDueDate,
    Guid ChangedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

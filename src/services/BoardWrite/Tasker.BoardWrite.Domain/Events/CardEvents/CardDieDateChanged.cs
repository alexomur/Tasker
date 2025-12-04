using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardDueDateChanged(
    Guid BoardId,
    Guid CardId,
    DateTimeOffset? NewDueDate,
    DateTimeOffset OccurredAt
) : IDomainEvent;
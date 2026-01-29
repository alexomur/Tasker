using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardDeleted(
    Guid BoardId,
    Guid CardId,
    Guid DeletedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

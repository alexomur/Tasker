using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardAssigneesChanged(
    Guid BoardId,
    Guid CardId,
    IReadOnlyCollection<Guid> AssigneeUserIds,
    Guid ChangedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

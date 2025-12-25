using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record BoardCreated(
    Guid BoardId,
    Guid OwnerUserId,
    string Title,
    string? Description,
    DateTimeOffset OccurredAt
) : IDomainEvent;

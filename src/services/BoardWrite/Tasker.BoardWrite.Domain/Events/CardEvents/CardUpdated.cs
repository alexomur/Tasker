using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardUpdated(
    Guid BoardId,
    Guid CardId,
    string Title,
    string? Description,
    Guid UpdatedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

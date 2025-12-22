using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record LabelCreated(
    Guid BoardId,
    Guid LabelId,
    string Title,
    string? Description,
    string Color,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

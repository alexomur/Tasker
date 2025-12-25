using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.CardEvents;

public sealed record CardCreated(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

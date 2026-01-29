using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Events.BoardEvents;

public sealed record BoardMemberAdded(
    Guid BoardId,
    Guid UserId,
    BoardMemberRole Role,
    Guid AddedByUserId,
    DateTimeOffset OccurredAt
) : IDomainEvent;

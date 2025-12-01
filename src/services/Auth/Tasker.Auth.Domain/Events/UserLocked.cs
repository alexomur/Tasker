using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserLocked(Guid UserId, string? Reason, DateTimeOffset OccurredAt) : IDomainEvent;
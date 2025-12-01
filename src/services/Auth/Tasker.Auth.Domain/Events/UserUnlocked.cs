using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserUnlocked(Guid UserId, DateTimeOffset OccurredAt) : IDomainEvent;
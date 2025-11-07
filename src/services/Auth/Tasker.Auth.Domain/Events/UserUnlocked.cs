using Tasker.Auth.Domain.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserUnlocked(Guid UserId, DateTimeOffset OccurredAt) : IDomainEvent;
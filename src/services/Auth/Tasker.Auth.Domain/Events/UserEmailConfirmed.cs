using Tasker.Auth.Domain.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserEmailConfirmed(Guid UserId, DateTimeOffset OccurredAt) : IDomainEvent;
using Tasker.Auth.Domain.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserPasswordChanged(Guid UserId, DateTimeOffset OccurredAt) : IDomainEvent;
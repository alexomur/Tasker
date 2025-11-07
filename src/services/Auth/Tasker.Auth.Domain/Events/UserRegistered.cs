using Tasker.Auth.Domain.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserRegistered(Guid UserId, string Email, string DisplayName, DateTimeOffset OccurredAt) : IDomainEvent;
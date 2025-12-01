using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserEmailConfirmed(Guid UserId, DateTimeOffset OccurredAt) : IDomainEvent;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserEmailChanged(Guid UserId, string NewEmail, DateTimeOffset OccurredAt) : IDomainEvent;
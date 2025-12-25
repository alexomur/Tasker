using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserLoginSucceeded(
    Guid UserId,
    string Email,
    DateTimeOffset OccurredAt
) : IDomainEvent;

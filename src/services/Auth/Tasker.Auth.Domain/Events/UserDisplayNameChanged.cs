using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Events;

public sealed record UserDisplayNameChanged(Guid UserId, string NewDisplayName, DateTimeOffset OccurredAt) : IDomainEvent;
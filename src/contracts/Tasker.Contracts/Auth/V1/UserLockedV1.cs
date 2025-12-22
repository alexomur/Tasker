namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user has been locked.
/// </summary>
public sealed record UserLockedV1(
    Guid UserId,
    string? Reason,
    DateTimeOffset OccurredAt
);

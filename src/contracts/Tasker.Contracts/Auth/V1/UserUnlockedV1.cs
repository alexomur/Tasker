namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user has been unlocked.
/// </summary>
public sealed record UserUnlockedV1(
    Guid UserId,
    DateTimeOffset OccurredAt
);

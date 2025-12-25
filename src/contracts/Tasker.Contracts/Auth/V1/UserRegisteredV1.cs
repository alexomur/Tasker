namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: a user has been registered.
/// </summary>
public sealed record UserRegisteredV1(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTimeOffset OccurredAt
);

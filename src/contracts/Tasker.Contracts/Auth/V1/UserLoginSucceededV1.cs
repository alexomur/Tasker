namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user login succeeded.
/// </summary>
public sealed record UserLoginSucceededV1(
    Guid UserId,
    string Email,
    DateTimeOffset OccurredAt
);

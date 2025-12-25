namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user email has been changed.
/// </summary>
public sealed record UserEmailChangedV1(
    Guid UserId,
    string NewEmail,
    DateTimeOffset OccurredAt
);

namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user password has been changed.
/// </summary>
public sealed record UserPasswordChangedV1(
    Guid UserId,
    DateTimeOffset OccurredAt
);

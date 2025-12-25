namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user email has been confirmed.
/// </summary>
public sealed record UserEmailConfirmedV1(
    Guid UserId,
    DateTimeOffset OccurredAt
);

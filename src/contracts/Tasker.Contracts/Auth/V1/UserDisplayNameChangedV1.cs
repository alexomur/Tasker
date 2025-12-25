namespace Tasker.Contracts.Auth.V1;

/// <summary>
/// Integration event: user display name has been changed.
/// </summary>
public sealed record UserDisplayNameChangedV1(
    Guid UserId,
    string NewDisplayName,
    DateTimeOffset OccurredAt
);

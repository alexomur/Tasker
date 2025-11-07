namespace Tasker.Auth.Domain.Sessions;

public sealed record AuthSession(Guid UserId, DateTimeOffset CreatedAt);
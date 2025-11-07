namespace Tasker.Auth.Domain.Tokens;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? RevokedAt { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset revokedAt, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
        CreatedAt = createdAt;
    }
    

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset issuedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException($"'{nameof(tokenHash)}' cannot be null or empty.", nameof(tokenHash));
        }

        if (expiresAt < issuedAt)
        {
            throw new ArgumentException("Expiration must be in the future.", nameof(expiresAt));
        }
        
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAt, issuedAt, DateTimeOffset.UtcNow);
    }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAt is not null)
        {
            return;
        }
        RevokedAt = now;
    }
}
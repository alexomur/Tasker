using System.Security.Cryptography;
using System.Text.Json;
using StackExchange.Redis;
using Tasker.Auth.Application.Abstractions.Sessions;
using Tasker.Auth.Domain.Sessions;

namespace Tasker.Auth.Infrastructure.Sessions;

public class RedisAuthSessionStore : IAuthSessionStore
{
    private readonly IDatabase _db;

    public RedisAuthSessionStore(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }
    
    public async Task<string> CreateAsync(Guid userId, TimeSpan ttl, CancellationToken cancellationToken)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToHexString(bytes);

        var session = new AuthSession(userId, DateTimeOffset.UtcNow);
        var payload = JsonSerializer.Serialize(session);

        await _db.StringSetAsync(Key(token), payload, ttl);
        return token;
    }

    public async Task<AuthSession?> GetAsync(string token, CancellationToken cancellationToken)
    {
        var val = await _db.StringGetAsync(Key(token));
        if (val.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<AuthSession>(val!);
    }

    public Task<bool> DeleteAsync(string token, CancellationToken cancellationToken) =>
        _db.KeyDeleteAsync(Key(token));

    private static string Key(string token) => $"auth:sessions:{token}";
}
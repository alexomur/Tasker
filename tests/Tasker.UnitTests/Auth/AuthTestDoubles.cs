using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Application.Abstractions.Sessions;
using Tasker.Auth.Domain.Sessions;
using Tasker.Auth.Domain.Users;
using Tasker.Shared.Kernel.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Tasker.UnitTests.Auth;

internal sealed class FakeUserRepository : IUserRepository
{
    // Ключ – email.Value
    private readonly Dictionary<string, User> _byEmail = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<User> Users => _byEmail.Values.ToList();

    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = _byEmail.Values.SingleOrDefault(u => u.Id == userId);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        _byEmail.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<bool> ExistUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var exists = _byEmail.ContainsKey(email);
        return Task.FromResult(exists);
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        AddUser(user);
        return Task.CompletedTask;
    }

    public void AddUser(User user)
    {
        _byEmail[user.Email.Value] = user;
    }
}

internal sealed class FakePasswordService : IPasswordService
{
    public List<(User User, string Password)> HashCalls { get; } = new();
    public List<(User User, string Hash, string Password)> VerifyCalls { get; } = new();

    /// <summary>
    /// Результат, который вернёт Verify (по умолчанию — Success).
    /// </summary>
    public PasswordVerificationResult VerifyResult { get; set; } = PasswordVerificationResult.Success;

    public string Hash(User user, string password)
    {
        HashCalls.Add((user, password));
        return $"HASHED:{password}";
    }

    public PasswordVerificationResult Verify(User user, string hash, string password)
    {
        VerifyCalls.Add((user, hash, password));
        return VerifyResult;
    }
}

internal sealed class FakeAuthSessionStore : IAuthSessionStore
{
    public List<(Guid UserId, TimeSpan Ttl)> Created { get; } = new();

    public string TokenToReturn { get; set; } = "test-session-token";

    public Task<string> CreateAsync(Guid userId, TimeSpan ttl, CancellationToken cancellationToken)
    {
        Created.Add((userId, ttl));
        return Task.FromResult(TokenToReturn);
    }

    public Task<AuthSession?> GetAsync(string token, CancellationToken cancellationToken)
    {
        // Для наших тестов не нужно – можно вернуть null
        return Task.FromResult<AuthSession?>(null);
    }

    public Task<bool> DeleteAsync(string token, CancellationToken cancellationToken)
    {
        // Для наших тестов не важно – считаем, что успешно
        return Task.FromResult(true);
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Func<CancellationToken, Task<int>>? OnSaveChangesAsync { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (OnSaveChangesAsync is not null)
        {
            return OnSaveChangesAsync(cancellationToken);
        }

        return Task.FromResult(1);
    }
}

using Microsoft.AspNetCore.Identity;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    
    public string Hash(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public PasswordVerificationResult Verify(User user, string hash, string password)
    {
        return _hasher.VerifyHashedPassword(user, hash, password);
    }
}
using Microsoft.AspNetCore.Identity;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Application.Abstractions.Security;

public interface IPasswordService
{
    string Hash(User user, string password);
    
    PasswordVerificationResult Verify(User user, string hash, string password);
}
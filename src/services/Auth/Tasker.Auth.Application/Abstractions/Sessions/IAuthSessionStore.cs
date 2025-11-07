using Tasker.Auth.Domain.Sessions;

namespace Tasker.Auth.Application.Abstractions.Sessions;

public interface IAuthSessionStore
{
    Task<string> CreateAsync(Guid userId, TimeSpan ttl, CancellationToken cancellationToken);
    
    Task<AuthSession?> GetAsync(string token, CancellationToken cancellationToken);
        
    Task<bool> DeleteAsync(string token, CancellationToken cancellationToken);
}
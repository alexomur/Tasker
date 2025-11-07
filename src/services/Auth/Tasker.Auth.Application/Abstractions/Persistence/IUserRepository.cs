using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    
    Task<bool> ExistUserByEmailAsync(string email, CancellationToken cancellationToken);
    
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    
    void AddUser(User user);
}
namespace Tasker.Core.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
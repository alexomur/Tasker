using Microsoft.EntityFrameworkCore;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db)
    {
        _db = db;
    }
    
    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public Task<bool> ExistUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _db.Users.AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        _db.Users.AddAsync(user, cancellationToken);
        return Task.CompletedTask;
    }
}
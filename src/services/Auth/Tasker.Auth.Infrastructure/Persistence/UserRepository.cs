using Microsoft.EntityFrameworkCore;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Domain.Users;
using Tasker.Auth.Domain.ValueObjects;

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
    }public async Task<bool> ExistUserByEmailAsync(string email, CancellationToken ct)
    {
        var vo = EmailAddress.Create(email);
        return await _db.Users.AsNoTracking()
            .AnyAsync(u => u.Email == vo, ct);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var vo = EmailAddress.Create(email);
        return _db.Users
            .SingleOrDefaultAsync(u => u.Email == vo, ct);
    }

    public async Task AddUserAsync(User user, CancellationToken ct)
    {
        await _db.Users.AddAsync(user, ct);
    }
    
    public void AddUser(User user) => _db.Users.Add(user);
}
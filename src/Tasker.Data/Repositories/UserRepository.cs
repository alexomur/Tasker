using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasker.Core.Users;

namespace Tasker.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TaskerDbContext db) : base(db)
    {
        
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        return user;
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, ct);

        return user;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var exists = await _db.Set<User>()
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);

        return exists;
    }
}
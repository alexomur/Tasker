using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Tasker.Auth.Infrastructure;
using Tasker.BoardRead.Application.Users.Abstractions;
using Tasker.BoardRead.Application.Users.Views;

namespace Tasker.BoardRead.Infrastructure.Users;

public sealed class AuthUserReadService : IUserReadService
{
    private readonly AuthDbContext _dbContext;

    public AuthUserReadService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UserView>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var normalizedIds = ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedIds.Length == 0)
        {
            return Array.Empty<UserView>();
        }

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => normalizedIds.Contains(u.Id))
            .Select(u => new UserView(
                u.Id,
                u.DisplayName,
                u.Email.Value))
            .ToListAsync(cancellationToken);


        return users;
    }
}
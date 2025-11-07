using MediatR;
using Microsoft.EntityFrameworkCore;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;

    public RegisterUserHandler(IUserRepository users, IUnitOfWork uow, IPasswordService pwd)
    {
        _users = users; _uow = uow; _pwd = pwd;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await _users.ExistUserByEmailAsync(cmd.Email, ct))
            throw new InvalidOperationException("Email already registered.");

        var now = DateTimeOffset.UtcNow;
        var user = User.Register(cmd.Email, cmd.DisplayName, "__placeholder__", now);
        var hash = _pwd.Hash(user, cmd.Password);
        user.ChangePasswordHash(hash, now);

        _users.AddUser(user);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Duplicate") == true)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        return new RegisterUserResult(user.Id);
    }
}
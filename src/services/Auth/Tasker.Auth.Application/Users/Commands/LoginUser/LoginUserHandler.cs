using MediatR;
using Microsoft.AspNetCore.Identity;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Application.Abstractions.Sessions;

namespace Tasker.Auth.Application.Users.Commands.LoginUser;

public sealed class LoginUserHandler
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _pwd;
    private readonly IAuthSessionStore _sessions;
    private readonly IUnitOfWork _uow; // на случай rehash

    public LoginUserHandler(IUserRepository users, IPasswordService pwd, IAuthSessionStore sessions, IUnitOfWork uow)
    { _users = users; _pwd = pwd; _sessions = sessions; _uow = uow; }

    public async Task<LoginUserResult> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetUserByEmailAsync(cmd.Email, ct);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        if (user.IsLocked)
        {
            throw new UnauthorizedAccessException("User locked.");
        }
        if (!user.EmailConfirmed)
        {
            // throw new UnauthorizedAccessException("Email not confirmed.");
        }

        var result = _pwd.Verify(user, user.PasswordHash, cmd.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.ChangePasswordHash(_pwd.Hash(user, cmd.Password), DateTimeOffset.UtcNow);
            await _uow.SaveChangesAsync(ct);
        }

        var token = await _sessions.CreateAsync(user.Id, cmd.Ttl, ct);
        return new LoginUserResult(user.Id, token, (long)cmd.Ttl.TotalSeconds);
    }
}
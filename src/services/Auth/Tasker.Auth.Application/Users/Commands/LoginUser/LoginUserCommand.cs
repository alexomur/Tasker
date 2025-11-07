using MediatR;

namespace Tasker.Auth.Application.Users.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password, TimeSpan Ttl)
    : IRequest<LoginUserResult>;

public sealed record LoginUserResult(Guid UserId, string AccessToken, long ExpiresInSeconds);
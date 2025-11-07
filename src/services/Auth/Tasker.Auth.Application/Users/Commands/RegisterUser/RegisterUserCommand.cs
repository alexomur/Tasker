using MediatR;

namespace Tasker.Auth.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string DisplayName, string Password)
    : IRequest<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId);
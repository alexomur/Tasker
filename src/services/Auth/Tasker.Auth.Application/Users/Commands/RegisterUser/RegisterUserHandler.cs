using MediatR;
using Microsoft.EntityFrameworkCore;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Domain.Errors;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public RegisterUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _userRepository = userRepository; 
        _unitOfWork = unitOfWork; 
        _passwordService = passwordService;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await _userRepository.ExistUserByEmailAsync(cmd.Email, ct))
        {
            throw new EmailAlreadyRegisteredException(cmd.Email);
        }

        var now = DateTimeOffset.UtcNow;
        var user = User.Register(cmd.Email, cmd.DisplayName, "__placeholder__", now);
        var hash = _passwordService.Hash(user, cmd.Password);
        user.ChangePasswordHash(hash, now);

        _userRepository.AddUser(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Duplicate") == true)
        {
            throw new EmailAlreadyRegisteredException(cmd.Email);
        }

        return new RegisterUserResult(user.Id);
    }
}
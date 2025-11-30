using MediatR;
using Microsoft.AspNetCore.Identity;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Application.Abstractions.Security;
using Tasker.Auth.Application.Abstractions.Sessions;
using Tasker.Auth.Domain.Errors;

namespace Tasker.Auth.Application.Users.Commands.LoginUser;

public sealed class LoginUserHandler
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _userRepository;
    
    private readonly IPasswordService _passwordService;
    
    private readonly IAuthSessionStore _authSessionStore;
    
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserHandler(IUserRepository userRepository, IPasswordService passwordService,
        IAuthSessionStore authSessionStore, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository; 
        _passwordService = passwordService; 
        _authSessionStore = authSessionStore; 
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetUserByEmailAsync(cmd.Email, ct);
        
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }
        if (user.IsLocked)
        {
            throw new UserLockedException();
        }
        if (!user.EmailConfirmed)
        {
            // throw new UnauthorizedAccessException("Email not confirmed.");
        }

        var result = _passwordService.Verify(user, user.PasswordHash, cmd.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.ChangePasswordHash(_passwordService.Hash(user, cmd.Password), DateTimeOffset.UtcNow);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var token = await _authSessionStore.CreateAsync(user.Id, cmd.Ttl, ct);
        return new LoginUserResult(user.Id, token, (long)cmd.Ttl.TotalSeconds);
    }
}
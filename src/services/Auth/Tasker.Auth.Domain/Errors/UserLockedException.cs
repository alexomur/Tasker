namespace Tasker.Auth.Domain.Errors;

public class UserLockedException : AuthException
{
    public UserLockedException() : base("User is locked.", "auth.user_locked", 401)
    {
    }
}
namespace Tasker.Auth.Domain.Errors;

public sealed class InvalidCredentialsException : AuthException
{
    public InvalidCredentialsException() : 
        base("Invalid email or password.", "auth.invalid_credentials", 401)
    {
    }
}
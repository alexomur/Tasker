namespace Tasker.Auth.Domain.Errors;

public sealed class EmailAlreadyRegisteredException : AuthException
{
    public string Email { get; }

    public EmailAlreadyRegisteredException(string email) : 
        base(
            "Email already registered.", 
            "auth.email_already_registered", 
            409)
    {
        Email = email;
    }
}
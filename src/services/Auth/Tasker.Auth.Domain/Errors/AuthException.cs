using Tasker.Shared.Kernel.Errors;

namespace Tasker.Auth.Domain.Errors;

public abstract class AuthException : AppException
{
    protected AuthException(string message, string code, int statusCode) : base(message, code, statusCode)
    {
    }
}
namespace Tasker.Shared.Kernel.Errors;

public class AppException : Exception
{
    public string Code { get; }
    
    public int StatusCode { get; }
    
    public AppException(string message, string code, int statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
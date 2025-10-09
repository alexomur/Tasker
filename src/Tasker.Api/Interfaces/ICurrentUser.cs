namespace Tasker.Api.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    
    bool IsAuthenticated { get; }
    
    string? Username { get; }
}
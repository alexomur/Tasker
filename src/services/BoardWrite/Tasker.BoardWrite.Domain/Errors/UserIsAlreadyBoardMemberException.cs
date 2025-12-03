using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors;

public class UserIsAlreadyBoardMemberException : AppException
{
    public Guid UserId { get; }
    
    public UserIsAlreadyBoardMemberException(Guid userId) : base($"User with id '{userId}' is already board member", "board_write.user_already_boardmember", 409)
    {
        UserId = userId;
    }
}
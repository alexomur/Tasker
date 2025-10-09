using System;

namespace Tasker.Core.Boards;

public class BoardMember : Entity
{
    public Guid UserId { get; private set; }
    public BoardRole Role { get; private set; }

    protected BoardMember() { }

    public BoardMember(Guid userId, BoardRole role)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        UserId = userId;
        Role = role;
    }

    public void ChangeRole(BoardRole newRole)
    {
        Role = newRole;
    }
}
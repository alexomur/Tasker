namespace Tasker.BoardRead.Domain.UserViews;

public sealed record UserView(
    Guid Id,
    string DisplayName,
    string Email);
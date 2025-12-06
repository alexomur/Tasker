namespace Tasker.BoardRead.Application.Users.Views;

public sealed record UserView(
    Guid Id,
    string DisplayName,
    string Email);
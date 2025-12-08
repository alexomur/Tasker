namespace Tasker.BoardRead.Domain.BoardViews;

public sealed record BoardMemberView(
    Guid Id,
    Guid UserId,
    BoardMemberRole Role,
    bool IsActive,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt);
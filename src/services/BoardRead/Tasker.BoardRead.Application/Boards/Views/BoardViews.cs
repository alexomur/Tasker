using System;
using System.Collections.Generic;
using Tasker.BoardRead.Application.Users.Views;

namespace Tasker.BoardRead.Application.Boards.Views;

/// <summary>
/// Краткое представление доски для списка «Мои доски».
/// Соответствует фронтовому BoardListItem.
/// </summary>
public sealed record BoardView(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    BoardMemberRole MyRole);

/// <summary>
/// Детальное представление доски для отображения всей доски.
/// По форме совместимо с BoardDetailsResult и фронтовым BoardDetails.
/// </summary>
public sealed record BoardDetailsView(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<BoardColumnView> Columns,
    IReadOnlyCollection<BoardMemberView> Members,
    IReadOnlyCollection<BoardLabelView> Labels,
    IReadOnlyCollection<BoardCardView> Cards,
    IReadOnlyCollection<UserView> Users);

public sealed record BoardColumnView(
    Guid Id,
    string Title,
    string? Description,
    int Order);

public sealed record BoardMemberView(
    Guid Id,
    Guid UserId,
    BoardMemberRole Role,
    bool IsActive,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt);

public sealed record BoardLabelView(
    Guid Id,
    string Title,
    string? Description,
    string Color);

public sealed record BoardCardView(
    Guid Id,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DueDate,
    IReadOnlyCollection<Guid> AssigneeUserIds,
    IReadOnlyCollection<Guid> LabelIds);

// TODO: Move all authorization away from here
/// <summary>
/// Роль участника доски для read-слоя.
/// Значения совпадают с Tasker.BoardWrite.Domain.Boards.BoardMemberRole.
/// </summary>
public enum BoardMemberRole
{
    Owner = 0,
    Admin = 1,
    Member = 2,
    Viewer = 3
}
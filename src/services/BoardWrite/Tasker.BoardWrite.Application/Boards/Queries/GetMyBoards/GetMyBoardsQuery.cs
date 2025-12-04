using MediatR;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Boards.Queries.GetMyBoards;

/// <summary>
/// Запрос на получение списка досок текущего пользователя.
/// </summary>
public sealed record GetMyBoardsQuery
    : IRequest<IReadOnlyCollection<MyBoardListItemResult>>;

    /// <summary>
    /// Краткая информация о доске для списка «Мои доски».
    /// Будет использоваться фронтом и в будущем BoardRead.
    /// </summary>
public sealed record MyBoardListItemResult(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    BoardMemberRole MyRole);
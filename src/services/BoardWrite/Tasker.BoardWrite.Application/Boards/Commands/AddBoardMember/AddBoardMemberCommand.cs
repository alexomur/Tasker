using MediatR;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddBoardMember;

/// <summary>
/// Команда на добавление участника на доску.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="UserId">Идентификатор пользователя.</param>
/// <param name="Role">Роль участника на доске.</param>
public sealed record AddBoardMemberCommand(
    Guid BoardId,
    Guid UserId,
    BoardMemberRole Role
) : IRequest<AddBoardMemberResult>;

/// <summary>
/// Результат добавления участника.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="UserId">Идентификатор пользователя.</param>
/// <param name="Role">Роль участника.</param>
public sealed record AddBoardMemberResult(
    Guid BoardId,
    Guid UserId,
    BoardMemberRole Role);
using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.AssignMemberToCard;

/// <summary>
/// Команда на назначение участника (BoardMember) исполнителем по карточке.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="UserId">
/// Идентификатор пользователя, соответствующий участнику доски.
/// (BoardMember.UserId)
/// </param>
public sealed record AssignMemberToCardCommand(
    Guid BoardId,
    Guid CardId,
    Guid UserId
) : IRequest<AssignMemberToCardResult>;

/// <summary>
/// Результат назначения исполнителя.
/// </summary>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="UserId">Идентификатор пользователя-исполнителя.</param>
public sealed record AssignMemberToCardResult(
    Guid CardId,
    Guid UserId);
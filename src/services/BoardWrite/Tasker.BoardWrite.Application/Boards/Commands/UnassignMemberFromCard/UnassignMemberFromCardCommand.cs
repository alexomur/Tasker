using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.UnassignMemberFromCard;

/// <summary>
/// Команда на снятие участника с роли исполнителя по карточке.
/// </summary>
public sealed record UnassignMemberFromCardCommand(
    Guid BoardId,
    Guid CardId,
    Guid UserId
) : IRequest<UnassignMemberFromCardResult>;

/// <summary>
/// Результат снятия исполнителя.
/// </summary>
public sealed record UnassignMemberFromCardResult(
    Guid CardId,
    Guid UserId);
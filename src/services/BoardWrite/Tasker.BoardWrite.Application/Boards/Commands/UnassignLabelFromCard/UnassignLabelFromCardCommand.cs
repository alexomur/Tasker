using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.UnassignLabelFromCard;

public sealed record UnassignLabelFromCardCommand(
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IRequest<UnassignLabelFromCardResult>;

public sealed record UnassignLabelFromCardResult(
    Guid CardId,
    Guid LabelId);
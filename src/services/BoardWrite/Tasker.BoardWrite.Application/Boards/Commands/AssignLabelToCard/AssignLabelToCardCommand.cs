using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.AssignLabelToCard;

public sealed record AssignLabelToCardCommand(
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IRequest<AssignLabelToCardResult>;

public sealed record AssignLabelToCardResult(
    Guid CardId,
    Guid LabelId);
using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteCard;

public sealed record DeleteCardCommand(Guid BoardId, Guid CardId) : IRequest;
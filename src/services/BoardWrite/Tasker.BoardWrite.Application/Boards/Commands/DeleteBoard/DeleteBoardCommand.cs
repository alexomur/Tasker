using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteBoard;

public sealed record DeleteBoardCommand(Guid BoardId) : IRequest;

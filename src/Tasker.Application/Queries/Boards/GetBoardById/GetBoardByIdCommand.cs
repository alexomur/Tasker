using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Application.Queries.Boards.GetBoardById;

public record GetBoardByIdCommand(Guid BoardId) : IRequest<Result<BoardDto?>>;
using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Application.Commands.Boards.GetBoardById;

public record GetBoardByIdCommand(Guid BoardId) : IRequest<Result<BoardDto?>>;
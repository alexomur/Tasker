using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;

namespace Tasker.Application.Commands.Boards.DeleteBoard;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Result<BaseResponseDto>>;
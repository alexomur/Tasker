using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;

namespace Tasker.Application.Commands.Boards.UpdateBoard;

public record UpdateBoardCommand(Guid BoardId, string Title, string? Description) : IRequest<Result<BaseResponseDto>>;
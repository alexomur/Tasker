using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;

namespace Tasker.Application.Commands.Boards.CreateBoard;

public record CreateBoardCommand(string Title, string? Description) : IRequest<Result<BaseResponseDto>>;
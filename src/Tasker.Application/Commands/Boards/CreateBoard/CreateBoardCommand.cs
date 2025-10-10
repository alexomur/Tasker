using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Application.Commands.Boards.CreateBoard;

public record CreateBoardCommand(string Title, string? Description) : IRequest<Result<BoardDto?>>;
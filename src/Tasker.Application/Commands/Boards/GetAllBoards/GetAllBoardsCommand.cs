using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Application.Commands.Boards.GetAllBoards;

public record GetAllBoardsCommand() : IRequest<Result<List<BoardDto>>>;
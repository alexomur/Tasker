using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Application.Queries.Boards.GetAllBoards;

public record GetAllBoardsCommand() : IRequest<Result<List<BoardDto>>>;
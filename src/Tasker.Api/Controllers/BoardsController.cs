using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.Api.Controllers.Base;
using Tasker.Application.Commands.Boards.CreateBoard;
using Tasker.Application.Commands.Boards.DeleteBoard;
using Tasker.Application.Commands.Boards.GetAllBoards;
using Tasker.Application.Commands.Boards.GetBoardById;
using Tasker.Application.Commands.Boards.UpdateBoard;
using Tasker.Application.DTOs;
using Tasker.Application.DTOs.Boards;

namespace Tasker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class BoardsController : MediatorControllerBase
{
    public BoardsController(IMediator mediator) : base(mediator)
    {
        
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
        => await ExecuteCommand<GetAllBoardsCommand, List<BoardDto>>(new GetAllBoardsCommand());

    [HttpGet("{boardId:guid}")]
    public async Task<IActionResult> Get(Guid boardId)
        => await ExecuteCommand<GetBoardByIdCommand, BoardDto?>(new GetBoardByIdCommand(boardId));

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromQuery] string title, [FromQuery] string? description)
        => await ExecuteCommand<CreateBoardCommand, BoardDto>(new CreateBoardCommand(title, description));

    [HttpPut("{boardId:guid}")]
    public async Task<IActionResult> UpdateBoard(Guid boardId, [FromQuery] string title, [FromQuery] string? description)
        => await ExecuteCommand<UpdateBoardCommand, BaseResponseDto>(new UpdateBoardCommand(boardId, title, description));

    [HttpDelete("{boardId:guid}")]
    public async Task<IActionResult> DeleteBoard(Guid boardId)
        => await ExecuteCommand<DeleteBoardCommand, BaseResponseDto>(new DeleteBoardCommand(boardId));
}
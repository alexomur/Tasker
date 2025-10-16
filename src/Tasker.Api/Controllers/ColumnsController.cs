using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.Api.Controllers.Base;
using Tasker.Application.Commands.Columns.CreateColumn;
using Tasker.Application.Commands.Columns.DeleteColumn;
using Tasker.Application.DTOs;
using Tasker.Application.DTOs.Columns;
using Tasker.Application.Queries.Columns.GetAllColumnsByBoardId;

namespace Tasker.Api.Controllers;

[ApiController]
[Area("Boards")]
[Route("Api/[area]/{boardId:guid}/[controller]")]
public class ColumnsController : MediatorControllerBase
{
    public ColumnsController(IMediator mediator) : base(mediator)
    {
        
    }
    
    [HttpGet("All")]
    public async Task<IActionResult> GetAllByBoardId([FromRoute] Guid boardId)
        => await ExecuteCommand<GetAllColumnsCommand, List<ColumnDto>>(new GetAllColumnsCommand(boardId));

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromRoute] Guid boardId, [FromQuery] string title, [FromQuery] string? description)
        => await ExecuteCommand<CreateColumnCommand, ColumnDto?>(new CreateColumnCommand(boardId, title, description));

    [HttpDelete("{columnId:guid}/Delete")]
    public async Task<IActionResult> Delete([FromRoute] Guid boardId, [FromRoute] Guid columnId)
        => await ExecuteCommand<DeleteColumnCommand, BaseResponseDto>(new DeleteColumnCommand(boardId, columnId));
}
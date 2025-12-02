using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.BoardWrite.Api.Controllers.Boards.Models;
using Tasker.BoardWrite.Application.Boards.Commands.AddBoardMember;
using Tasker.BoardWrite.Application.Boards.Commands.AddColumn;
using Tasker.BoardWrite.Application.Boards.Commands.AddLabel;
using Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;
using Tasker.BoardWrite.Application.Boards.Commands.CreateCard;
using Tasker.BoardWrite.Application.Boards.Queries.GetBoardDetails;

namespace Tasker.BoardWrite.Api.Controllers.Boards;

/// <summary>
/// HTTP API для управления досками (write-сторона).
/// </summary>
[ApiController]
[Route("api/v1/boards")]
public sealed class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Создаёт новую доску.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBoardResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBoardResult>> CreateBoard(
        [FromBody] CreateBoardRequest request,
        CancellationToken ct)
    {
        var cmd = new CreateBoardCommand(
            Title: request.Title,
            OwnerUserId: request.OwnerUserId,
            Description: request.Description);

        var result = await _mediator.Send(cmd, ct);

        return CreatedAtAction(
            nameof(GetBoardById),
            new { boardId = result.BoardId },
            result);
    }

    /// <summary>
    /// Возвращает полную информацию о доске.
    /// </summary>
    [HttpGet("{boardId:guid}")]
    [ProducesResponseType(typeof(BoardDetailsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BoardDetailsResult>> GetBoardById(
        Guid boardId,
        CancellationToken ct)
    {
        var query = new GetBoardDetailsQuery(boardId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Добавляет новую колонку на доску.
    /// </summary>
    [HttpPost("{boardId:guid}/columns")]
    [ProducesResponseType(typeof(AddColumnResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<AddColumnResult>> AddColumn(
        Guid boardId,
        [FromBody] AddColumnRequest request,
        CancellationToken ct)
    {
        var cmd = new AddColumnCommand(
            BoardId: boardId,
            Title: request.Title,
            Description: request.Description);

        var result = await _mediator.Send(cmd, ct);

        return Created(
            $"/api/v1/boards/{boardId}/columns/{result.ColumnId}",
            result);
    }

    /// <summary>
    /// Добавляет участника на доску.
    /// </summary>
    [HttpPost("{boardId:guid}/members")]
    [ProducesResponseType(typeof(AddBoardMemberResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<AddBoardMemberResult>> AddMember(
        Guid boardId,
        [FromBody] AddBoardMemberRequest request,
        CancellationToken ct)
    {
        var cmd = new AddBoardMemberCommand(
            BoardId: boardId,
            UserId: request.UserId,
            Role: request.Role);

        var result = await _mediator.Send(cmd, ct);

        return Created(
            $"/api/v1/boards/{boardId}/members/{result.UserId}",
            result);
    }

    /// <summary>
    /// Добавляет метку на доску.
    /// </summary>
    [HttpPost("{boardId:guid}/labels")]
    [ProducesResponseType(typeof(AddLabelResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<AddLabelResult>> AddLabel(
        Guid boardId,
        [FromBody] AddLabelRequest request,
        CancellationToken ct)
    {
        var cmd = new AddLabelCommand(
            BoardId: boardId,
            Title: request.Title,
            Color: request.Color,
            Description: request.Description);

        var result = await _mediator.Send(cmd, ct);

        return Created(
            $"/api/v1/boards/{boardId}/labels/{result.LabelId}",
            result);
    }

    /// <summary>
    /// Создаёт новую карточку в указанной колонке на доске.
    /// </summary>
    [HttpPost("{boardId:guid}/cards")]
    [ProducesResponseType(typeof(CreateCardResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateCardResult>> CreateCard(
        Guid boardId,
        [FromBody] CreateCardRequest request,
        CancellationToken ct)
    {
        var cmd = new CreateCardCommand(
            BoardId: boardId,
            ColumnId: request.ColumnId,
            Title: request.Title,
            CreatedByUserId: request.CreatedByUserId,
            Description: request.Description,
            DueDate: request.DueDate);

        var result = await _mediator.Send(cmd, ct);

        return Created(
            $"/api/v1/boards/{boardId}/cards/{result.CardId}",
            result);
    }
}

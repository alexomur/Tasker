using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasker.BoardRead.Application.Boards.Queries.GetBoardDetails;
using Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;
using Tasker.BoardRead.Application.Boards.Views;

namespace Tasker.BoardRead.Api.Controllers.Boards;

/// <summary>
/// HTTP API для чтения досок (read-сторона).
/// </summary>
[ApiController]
[Route("api/v1/boards")]
[Authorize]
public sealed class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Возвращает список досок текущего пользователя.
    /// Соответствует BoardListItem на фронте.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BoardView>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<BoardView>>> GetMyBoards(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyBoardsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Возвращает полную информацию о доске.
    /// Соответствует BoardDetails на фронте.
    /// </summary>
    [HttpGet("{boardId:guid}")]
    [ProducesResponseType(typeof(BoardDetailsView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BoardDetailsView>> GetBoardById(
        Guid boardId,
        CancellationToken ct)
    {
        var view = await _mediator.Send(new GetBoardDetailsQuery(boardId), ct);
        if (view is null)
        {
            return NotFound(new { message = "Board not found" });
        }

        return Ok(view);
    }
}
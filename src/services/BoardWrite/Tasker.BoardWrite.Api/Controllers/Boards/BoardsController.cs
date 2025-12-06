using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasker.BoardWrite.Api.Controllers.Boards.Models;
using Tasker.BoardWrite.Application.Abstractions.Services;
using Tasker.BoardWrite.Application.Boards.Commands.AddBoardMember;
using Tasker.BoardWrite.Application.Boards.Commands.AddColumn;
using Tasker.BoardWrite.Application.Boards.Commands.AssignLabelToCard;
using Tasker.BoardWrite.Application.Boards.Commands.AssignMemberToCard;
using Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;
using Tasker.BoardWrite.Application.Boards.Commands.CreateCard;
using Tasker.BoardWrite.Application.Boards.Commands.CreateLabel;
using Tasker.BoardWrite.Application.Boards.Commands.MoveCard;
using Tasker.BoardWrite.Application.Boards.Commands.SetCardDueDate;
using Tasker.BoardWrite.Application.Boards.Commands.UnassignLabelFromCard;
using Tasker.BoardWrite.Application.Boards.Commands.UnassignMemberFromCard;
using Tasker.BoardWrite.Application.Boards.Commands.UpdateCard;
using Tasker.BoardWrite.Application.Boards.Queries.GetBoardDetails;
using Tasker.BoardWrite.Application.Boards.Queries.GetMyBoards;

namespace Tasker.BoardWrite.Api.Controllers.Boards;

/// <summary>
/// HTTP API для управления досками (write-сторона).
/// Временно содержит read-эндпоинты для MVP.
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
    /// В будущем будет перенесено в BoardRead.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyCollection<MyBoardListItemResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<MyBoardListItemResult>>> GetMyBoards(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyBoardsQuery(), ct);
        return Ok(result);
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
            Description: request.Description,
            TemplateCode: request.TemplateCode);

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
        var cmd = new CreateLabelCommand(
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
            Description: request.Description,
            DueDate: request.DueDate);

        var result = await _mediator.Send(cmd, ct);

        return Created(
            $"/api/v1/boards/{boardId}/cards/{result.CardId}",
            result);
    }

    /// <summary>
    /// Обновляет существующую карточку в рамках доски.
    /// </summary>
    [HttpPut("{boardId:guid}/cards/{cardId:guid}")]
    [ProducesResponseType(typeof(UpdateCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateCardResult>> UpdateCard(
        Guid boardId,
        Guid cardId,
        [FromBody] UpdateCardRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateCardCommand(
            BoardId: boardId,
            CardId: cardId,
            Title: request.Title,
            Description: request.Description);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Перемещает карточку в другую колонку.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/move")]
    [ProducesResponseType(typeof(MoveCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoveCardResult>> MoveCard(
        Guid boardId,
        Guid cardId,
        [FromBody] MoveCardRequest request,
        CancellationToken ct)
    {
        var cmd = new MoveCardCommand(
            BoardId: boardId,
            CardId: cardId,
            TargetColumnId: request.TargetColumnId);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Устанавливает или сбрасывает дедлайн карточки.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/due-date")]
    [ProducesResponseType(typeof(SetCardDueDateResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SetCardDueDateResult>> SetCardDueDate(
        Guid boardId,
        Guid cardId,
        [FromBody] SetCardDueDateRequest request,
        CancellationToken ct)
    {
        var cmd = new SetCardDueDateCommand(
            BoardId: boardId,
            CardId: cardId,
            DueDate: request.DueDate);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Назначает участника исполнителем по карточке.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/assignees")]
    [ProducesResponseType(typeof(AssignMemberToCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignMemberToCardResult>> AssignMemberToCard(
        Guid boardId,
        Guid cardId,
        [FromBody] CardAssigneeRequest request,
        CancellationToken ct)
    {
        var cmd = new AssignMemberToCardCommand(
            BoardId: boardId,
            CardId: cardId,
            UserId: request.UserId);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Снимает участника с роли исполнителя по карточке.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/assignees/remove")]
    [ProducesResponseType(typeof(UnassignMemberFromCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnassignMemberFromCardResult>> UnassignMemberFromCard(
        Guid boardId,
        Guid cardId,
        [FromBody] CardAssigneeRequest request,
        CancellationToken ct)
    {
        var cmd = new UnassignMemberFromCardCommand(
            BoardId: boardId,
            CardId: cardId,
            UserId: request.UserId);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }
    
    /// <summary>
    /// Назначает метку карточке.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/labels")]
    [ProducesResponseType(typeof(AssignLabelToCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignLabelToCardResult>> AssignLabelToCard(
        Guid boardId,
        Guid cardId,
        [FromBody] CardLabelRequest request,
        CancellationToken ct)
    {
        var cmd = new AssignLabelToCardCommand(
            BoardId: boardId,
            CardId: cardId,
            LabelId: request.LabelId);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Снимает метку с карточки.
    /// </summary>
    [HttpPost("{boardId:guid}/cards/{cardId:guid}/labels/remove")]
    [ProducesResponseType(typeof(UnassignLabelFromCardResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnassignLabelFromCardResult>> UnassignLabelFromCard(
        Guid boardId,
        Guid cardId,
        [FromBody] CardLabelRequest request,
        CancellationToken ct)
    {
        var cmd = new UnassignLabelFromCardCommand(
            BoardId: boardId,
            CardId: cardId,
            LabelId: request.LabelId);

        var result = await _mediator.Send(cmd, ct);

        return Ok(result);
    }

    /// <summary>
    /// Возвращает список доступных шаблонов досок.
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BoardTemplateDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<BoardTemplateDto>> GetTemplates(
        [FromServices] IBoardTemplateService templateService)
    {
        var list = templateService
            .GetTemplates()
            .Select(BoardTemplateDto.FromDomain)
            .ToArray();

        return Ok(list);
    }
}

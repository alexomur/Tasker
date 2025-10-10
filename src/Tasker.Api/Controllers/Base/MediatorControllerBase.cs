using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.Application.Commands.Boards.GetBoardById;
using Tasker.Application.Common;
using Tasker.Application.DTOs;

namespace Tasker.Api.Controllers.Base;

public abstract class MediatorControllerBase : ControllerBase
{
    protected readonly IMediator _mediator;

    protected MediatorControllerBase(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected Guid? GetUserId()
    {
        var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (nameIdentifierClaim == null)
        {
            var subClaim = User.FindFirst("sub");

            if (subClaim == null)
            {
                return null;
            }

            if (!Guid.TryParse(subClaim.Value, out var parsedSub))
            {
                return null;
            }

            return parsedSub;
        }

        if (!Guid.TryParse(nameIdentifierClaim.Value, out var parsedId))
        {
            return null;
        }

        return parsedId;
    }

    protected async Task<IActionResult> ExecuteCommand<TCommand, TResponse>(TCommand command)
    {
        if (command is null)
        {
            return Problem(detail: "Request is null.");
        }

        var resultRaw = await _mediator.Send(command);

        if (resultRaw is not Result<TResponse> result)
        {
            return BadRequest(new { Message = "Response is invalid." });
        }
        
        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error);
        }

        var payload = result.Value;

        if (payload == null)
        {
            return Problem(detail: "Result value is null.");
        }

        return Ok(payload);
    }
}
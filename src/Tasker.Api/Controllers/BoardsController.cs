using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.Api.Controllers.Base;

namespace Tasker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class BoardsController : MediatorControllerBase
{
    public BoardsController(IMediator mediator)
        : base(mediator)
    {
        
    }
}

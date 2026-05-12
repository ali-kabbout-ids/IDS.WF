using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rwd.WF.Application.Features.Workflow.Queries;

namespace Rwd.WF.API.Controllers;

[ApiController]
[Route("api/check")]
[AllowAnonymous]
public sealed class CheckController(IMediator mediator) : ControllerBase
{
    [HttpPost("mane3kanone/{applicationId:guid}")]
    public async Task<IActionResult> Mane3Kanone([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CheckMane3KanoneQuery(applicationId), cancellationToken);
        return Ok(new { hasMane3 = result.HasMane3, details = result.Details });
    }
}

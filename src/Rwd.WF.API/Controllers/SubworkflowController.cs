using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rwd.WF.Application.Features.Workflow.Commands;

namespace Rwd.WF.API.Controllers;

[ApiController]
[Route("api/subworkflow")]
[AllowAnonymous]
public sealed class SubworkflowController(IMediator mediator) : ControllerBase
{
    [HttpPost("result")]
    public async Task<IActionResult> PostResult([FromBody] SubWorkflowResultRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReceiveSubWorkflowResultCommand(body.ParentInstanceId, body.Result), cancellationToken);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public sealed record SubWorkflowResultRequest(string ParentInstanceId, string Result);

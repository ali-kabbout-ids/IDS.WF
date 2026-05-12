using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rwd.WF.Application.Features.Workflow.Commands;
using Rwd.WF.Application.Features.Workflow.Queries;

namespace Rwd.WF.API.Controllers;

[ApiController]
[Route("api/workflow")]
[AllowAnonymous]
public sealed class WorkflowController(IMediator mediator) : ControllerBase
{
    [HttpPost("start/{definitionId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(
        [FromRoute] string definitionId,
        [FromQuery] string loggedInUser,
        [FromQuery] Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StartWorkflowCommand(definitionId, applicationId, loggedInUser), cancellationToken);
        return result.IsSuccess ? Ok(new { instanceId = result.Value }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox([FromQuery] string loggedInUser, CancellationToken cancellationToken)
    {
        var items = await mediator.Send(new GetInboxQuery(loggedInUser), cancellationToken);
        return Ok(items);
    }

    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetTask(
        [FromRoute] Guid taskId,
        [FromQuery] string loggedInUser,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTaskDetailQuery(taskId, loggedInUser), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("task/{taskId:guid}/claim")]
    public async Task<IActionResult> Claim(
        [FromRoute] Guid taskId,
        [FromQuery] string loggedInUser,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ClaimTaskCommand(taskId, loggedInUser), cancellationToken);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("task/{taskId:guid}/action")]
    public async Task<IActionResult> Action(
        [FromRoute] Guid taskId,
        [FromQuery] string loggedInUser,
        [FromBody] WorkflowTaskActionRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SubmitTaskActionCommand(taskId, body.Action, loggedInUser), cancellationToken);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("task/{taskId:guid}/save")]
    public async Task<IActionResult> Save(
        [FromRoute] Guid taskId,
        [FromQuery] string loggedInUser,
        [FromBody] WorkflowTaskSaveRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SaveTaskDraftCommand(taskId, body.FormData, loggedInUser), cancellationToken);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public sealed record WorkflowTaskActionRequest(string Action);

public sealed record WorkflowTaskSaveRequest(object FormData);

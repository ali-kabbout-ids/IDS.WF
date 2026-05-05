using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rwd.WF.API.Authorization;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Features.LookupItems.Commands;
using Rwd.WF.Application.Features.LookupItems.Queries.GetAll;
using Rwd.WF.Application.Features.LookupItems.Queries;

namespace Rwd.WF.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/lookup-items")]
[ApiVersion("1.0")]
[Authorize]
public class LookupItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.LookupRead)]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllLookupItemsQuery(categoryId), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.LookupRead)]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLookupItemByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.LookupCreate)]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLookupItemCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id, version = "1.0" }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.LookupUpdate)]
    [ProducesResponseType(typeof(LookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookupItemRequest request, CancellationToken ct)
    {
        var command = new UpdateLookupItemCommand(
            id,
            request.NameEn,
            request.NameAr,
            request.Value,
            request.DisplayOrder,
            request.IsActive);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.LookupDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteLookupItemCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }
}

public record UpdateLookupItemRequest(
    string NameEn,
    string NameAr,
    string? Value,
    int DisplayOrder,
    bool IsActive);


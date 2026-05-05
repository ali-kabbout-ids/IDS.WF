using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rwd.WF.API.Authorization;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Features.LookupCategories.Commands.Create;
using Rwd.WF.Application.Features.LookupCategories.Commands;
using Rwd.WF.Application.Features.LookupCategories.Queries;

namespace Rwd.WF.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/lookup-categories")]
[ApiVersion("1.0")]
[Authorize]
public class LookupCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.LookupRead)]
    [ProducesResponseType(typeof(IReadOnlyList<LookupCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllLookupCategoriesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.LookupRead)]
    [ProducesResponseType(typeof(LookupCategoryWithItemsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLookupCategoryByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.LookupCreate)]
    [ProducesResponseType(typeof(LookupCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLookupCategoryCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id, version = "1.0" }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.LookupUpdate)]
    [ProducesResponseType(typeof(LookupCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookupCategoryRequest request, CancellationToken ct)
    {
        var command = new UpdateLookupCategoryCommand(
            id,
            request.NameEn,
            request.NameAr,
            request.Description,
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
        var result = await mediator.Send(new DeleteLookupCategoryCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }
}

public record UpdateLookupCategoryRequest(
    string NameEn,
    string NameAr,
    string? Description,
    int DisplayOrder,
    bool IsActive);


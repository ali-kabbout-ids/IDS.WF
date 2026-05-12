using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Domain;
using Rwd.WF.Domain.Common;

namespace Rwd.WF.Application.Features.Workflow.Queries;

public record GetTaskDetailQuery(Guid TaskId, string LoggedInUser) : IRequest<Result<TaskDetailDto>>;

public sealed class GetTaskDetailQueryHandler(IAppDbContext db) : IRequestHandler<GetTaskDetailQuery, Result<TaskDetailDto>>
{
    public async Task<Result<TaskDetailDto>> Handle(GetTaskDetailQuery request, CancellationToken cancellationToken)
    {
        var role = WorkflowRoleHelper.Parse(request.LoggedInUser)
            ?? throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var roleString = WorkflowRoleHelper.ToRoleString(role);

        var task = await db.WorkflowTasks
            .AsNoTracking()
            .Include(t => t.Application)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
            return Result<TaskDetailDto>.NotFound("Task not found.");

        if (task.RequiredRole != roleString)
            return Result<TaskDetailDto>.Forbidden("This task is not assigned to your role.");

        var visible = task.Status == "Pending" ||
                      (task.Status == "Claimed" && task.ClaimedBy == request.LoggedInUser);
        if (!visible)
            return Result<TaskDetailDto>.Forbidden("You do not have access to this task.");

        var actions = JsonSerializer.Deserialize<List<string>>(task.AvailableActions) ?? [];
        object? appData = null;
        if (task.Application is not null)
        {
            try
            {
                appData = JsonSerializer.Deserialize<JsonElement>(task.Application.FormData);
            }
            catch
            {
                appData = task.Application.FormData;
            }
        }

        var dto = new TaskDetailDto(
            task.Id,
            task.ApplicationId,
            task.StepName,
            task.FormKey,
            actions,
            appData,
            task.Status);

        return Result<TaskDetailDto>.Success(dto);
    }
}

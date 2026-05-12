using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain;
using Rwd.WF.Domain.Common;

namespace Rwd.WF.Application.Features.Workflow.Commands;

public record SubmitTaskActionCommand(Guid TaskId, string Action, string LoggedInUser) : IRequest<Result>;

public sealed class SubmitTaskActionCommandHandler(IAppDbContext db, IWorkflowEngine workflowEngine)
    : IRequestHandler<SubmitTaskActionCommand, Result>
{
    public async Task<Result> Handle(SubmitTaskActionCommand request, CancellationToken cancellationToken)
    {
        var role = WorkflowRoleHelper.Parse(request.LoggedInUser)
            ?? throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var roleString = WorkflowRoleHelper.ToRoleString(role);

        var task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);
        if (task is null)
            return Result.NotFound("Task not found.");

        if (task.RequiredRole != roleString)
            return Result.Forbidden("This task is not assigned to your role.");

        if (task.Status != "Claimed" || task.ClaimedBy != request.LoggedInUser)
            return Result.Failure("You must claim this task before submitting an action.", 409);

        var allowed = JsonSerializer.Deserialize<List<string>>(task.AvailableActions) ?? [];
        if (!allowed.Any(a => string.Equals(a, request.Action, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure("The selected action is not allowed for this task.");

        if (string.IsNullOrWhiteSpace(task.ElsaTaskId))
            return Result.Failure("Task is missing workflow bookmark information.");

        task.SubmittedAction = request.Action;
        task.Status = "Completed";

        await db.SaveChangesAsync(cancellationToken);

        await workflowEngine.ResumeBookmarkAsync(task.ElsaTaskId, request.Action, cancellationToken);

        return Result.Success();
    }
}

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain;
using Rwd.WF.Domain.Common;

namespace Rwd.WF.Application.Features.Workflow.Commands;

public record SaveTaskDraftCommand(Guid TaskId, object FormData, string LoggedInUser) : IRequest<Result>;

public sealed class SaveTaskDraftCommandHandler(IAppDbContext db) : IRequestHandler<SaveTaskDraftCommand, Result>
{
    public async Task<Result> Handle(SaveTaskDraftCommand request, CancellationToken cancellationToken)
    {
        var role = WorkflowRoleHelper.Parse(request.LoggedInUser)
            ?? throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var roleString = WorkflowRoleHelper.ToRoleString(role);

        var task = await db.WorkflowTasks
            .Include(t => t.Application)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
            return Result.NotFound("Task not found.");

        if (task.RequiredRole != roleString)
            return Result.Forbidden("This task is not assigned to your role.");

        if (task.Application is null)
            return Result.NotFound("Application not found for this task.");

        task.Application.FormData = JsonSerializer.Serialize(request.FormData);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

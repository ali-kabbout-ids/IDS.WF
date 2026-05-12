using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain;
using Rwd.WF.Domain.Common;

namespace Rwd.WF.Application.Features.Workflow.Commands;

public record ClaimTaskCommand(Guid TaskId, string LoggedInUser) : IRequest<Result>;

public sealed class ClaimTaskCommandHandler(IAppDbContext db) : IRequestHandler<ClaimTaskCommand, Result>
{
    public async Task<Result> Handle(ClaimTaskCommand request, CancellationToken cancellationToken)
    {
        var role = WorkflowRoleHelper.Parse(request.LoggedInUser)
            ?? throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var roleString = WorkflowRoleHelper.ToRoleString(role);

        var task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);
        if (task is null)
            return Result.NotFound("Task not found.");

        if (task.RequiredRole != roleString)
            return Result.Forbidden("This task is not assigned to your role.");

        if (task.Status != "Pending")
            return Result.Failure("Only pending tasks can be claimed.", 409);

        task.Status = "Claimed";
        task.ClaimedBy = request.LoggedInUser;
        task.ClaimedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

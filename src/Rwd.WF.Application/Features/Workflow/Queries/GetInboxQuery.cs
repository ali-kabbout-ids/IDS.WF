using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Domain;

namespace Rwd.WF.Application.Features.Workflow.Queries;

public record GetInboxQuery(string LoggedInUser) : IRequest<IReadOnlyList<TaskSummaryDto>>;

public sealed class GetInboxQueryHandler(IAppDbContext db) : IRequestHandler<GetInboxQuery, IReadOnlyList<TaskSummaryDto>>
{
    public async Task<IReadOnlyList<TaskSummaryDto>> Handle(GetInboxQuery request, CancellationToken cancellationToken)
    {
        var role = WorkflowRoleHelper.Parse(request.LoggedInUser)
            ?? throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var roleString = WorkflowRoleHelper.ToRoleString(role);

        var tasks = await db.WorkflowTasks
            .AsNoTracking()
            .Where(t =>
                t.RequiredRole == roleString &&
                (t.Status == "Pending" || (t.Status == "Claimed" && t.ClaimedBy == request.LoggedInUser)))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskSummaryDto(t.Id, t.ApplicationId, t.StepName, t.FormKey, t.Status, t.ClaimedBy))
            .ToListAsync(cancellationToken);

        return tasks;
    }
}

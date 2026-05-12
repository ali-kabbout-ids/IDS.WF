using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Exceptions;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain;
using Rwd.WF.Domain.Common;

namespace Rwd.WF.Application.Features.Workflow.Commands;

public record StartWorkflowCommand(string DefinitionId, Guid ApplicationId, string LoggedInUser)
    : IRequest<Result<string>>;

public sealed class StartWorkflowCommandHandler(IAppDbContext db, IWorkflowEngine workflowEngine)
    : IRequestHandler<StartWorkflowCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (WorkflowRoleHelper.Parse(request.LoggedInUser) is null)
            throw new UnauthorizedException("Unknown workflow role for the supplied user.");

        var appExists = await db.Applications.AnyAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (!appExists)
            return Result<string>.NotFound("Application not found.");

        var instanceId = await workflowEngine.StartWorkflowAsync(request.DefinitionId, request.ApplicationId, cancellationToken);
        return Result<string>.Success(instanceId);
    }
}

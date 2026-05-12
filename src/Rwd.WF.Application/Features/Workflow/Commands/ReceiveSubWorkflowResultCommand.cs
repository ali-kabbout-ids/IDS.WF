using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Application.Features.Workflow.Commands;

public record ReceiveSubWorkflowResultCommand(string ParentInstanceId, string Result)
    : IRequest<Result>;

public sealed class ReceiveSubWorkflowResultCommandHandler(IAppDbContext db)
    : IRequestHandler<ReceiveSubWorkflowResultCommand, Result>
{
    public async Task<Result> Handle(ReceiveSubWorkflowResultCommand request, CancellationToken cancellationToken)
    {
        db.SubWorkflowResults.Add(new SubWorkflowResult
        {
            Id = Guid.NewGuid(),
            ParentInstanceId = request.ParentInstanceId,
            Result = request.Result,
            CompletedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

namespace Rwd.WF.Application.Common.Interfaces;

public interface IWorkflowEngine
{
    Task<string> StartWorkflowAsync(string definitionId, Guid applicationId, CancellationToken cancellationToken = default);

    Task ResumeBookmarkAsync(string bookmarkId, string selectedAction, CancellationToken cancellationToken = default);
}

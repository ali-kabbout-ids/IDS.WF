using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Parameters;
using Microsoft.Extensions.Logging;
using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.Infrastructure.Workflow;

public sealed class WorkflowEngine(
    IWorkflowRuntime workflowRuntime,
    IWorkflowResumer workflowResumer,
    ILogger<WorkflowEngine> logger) : IWorkflowEngine
{
    public async Task<string> StartWorkflowAsync(string definitionId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var input = new Dictionary<string, object>(StringComparer.Ordinal) { ["applicationId"] = applicationId };

        var response = await workflowRuntime.TryStartWorkflowAsync(
            definitionId,
            new StartWorkflowRuntimeParams
            {
                Input = input,
                CancellationToken = cancellationToken
            });

        if (response is null || string.IsNullOrWhiteSpace(response.WorkflowInstanceId))
        {
            logger.LogWarning("Workflow start did not return an instance id for definition {DefinitionId}", definitionId);
            throw new InvalidOperationException("Workflow could not be started for the given definition.");
        }

        return response.WorkflowInstanceId;
    }

    public Task ResumeBookmarkAsync(string bookmarkId, string selectedAction, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["SelectedAction"] = selectedAction
        };

        return workflowResumer.ResumeAsync(bookmarkId, payload, cancellationToken);
    }
}

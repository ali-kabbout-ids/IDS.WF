using System.Text.Json;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;
using Rwd.WF.Domain.Entities;
using Rwd.WF.Infrastructure.Persistence;

namespace Rwd.WF.Infrastructure.ElsaActivities;

[Activity("Generic", "GenericUserTask", DisplayName = "Generic User Task")]
public class GenericUserTaskActivity : Activity
{
    public const string BookmarkName = "GenericUserTask";

    [Input(Description = "Role required to action this task")]
    public Input<string> RequiredRole { get; set; } = default!;

    [Input(Description = "Comma-separated list of available actions e.g. Approve,Reject")]
    public Input<string> AvailableActions { get; set; } = default!;

    [Input(Description = "Form key from Admin Portal")]
    public Input<string> FormKey { get; set; } = default!;

    [Input(Description = "Step name identifier")]
    public Input<string> StepName { get; set; } = default!;

    [Output]
    public Output<string> SelectedAction { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var requiredRole = (context.Get(RequiredRole) ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(requiredRole))
            throw new InvalidOperationException("RequiredRole must be set for Generic User Task.");

        var availableCsv = context.Get(AvailableActions) ?? string.Empty;
        var formKey = context.Get(FormKey) ?? string.Empty;
        var stepName = context.Get(StepName) ?? string.Empty;

        var applicationId = ResolveApplicationId(context);
        var actionsArray = availableCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        var actionsJson = JsonSerializer.Serialize(actionsArray);

        var taskId = Guid.NewGuid();
        var task = new WorkflowTask
        {
            Id = taskId,
            ElsaTaskId = string.Empty,
            ApplicationId = applicationId,
            RequiredRole = requiredRole,
            AvailableActions = actionsJson,
            FormKey = formKey,
            StepName = stepName,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var serviceScopeFactory = context.GetRequiredService<IServiceScopeFactory>();
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.WorkflowTasks.Add(task);
        await db.SaveChangesAsync(context.CancellationToken);

        var bookmarkArgs = new CreateBookmarkArgs
        {
            BookmarkName = BookmarkName,
            Stimulus = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["TaskId"] = taskId.ToString("N"),
                ["ActivityExecutionId"] = context.Id
            },
            Callback = ResumeAsync,
            AutoBurn = true
        };

        var bookmark = context.CreateBookmark(bookmarkArgs);

        task.ElsaTaskId = bookmark.Id;
        await db.SaveChangesAsync(context.CancellationToken);
    }

    private static Guid ResolveApplicationId(ActivityExecutionContext context)
    {
        var input = context.WorkflowExecutionContext.Input;
        if (!input.TryGetValue("applicationId", out var raw) || raw is null)
            throw new InvalidOperationException("Workflow input must include an 'applicationId' when using Generic User Task.");

        return raw switch
        {
            Guid g => g,
            string s when Guid.TryParse(s, out var id) => id,
            _ => Guid.Parse(raw.ToString()!)
        };
    }

    private async ValueTask ResumeAsync(ActivityExecutionContext context)
    {
        var input = context.WorkflowInput;
        var action = input.TryGetValue("SelectedAction", out var v) ? v?.ToString() : null;
        if (string.IsNullOrWhiteSpace(action))
            action = string.Empty;

        context.Set(SelectedAction, action);
        await context.CompleteActivityAsync();
    }
}

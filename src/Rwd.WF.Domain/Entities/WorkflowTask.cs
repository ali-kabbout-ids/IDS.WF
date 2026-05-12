namespace Rwd.WF.Domain.Entities;

public sealed class WorkflowTask
{
    public Guid Id { get; set; }
    public string ElsaTaskId { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
    public string AvailableActions { get; set; } = "[]";
    public string FormKey { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ClaimedBy { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public string? SubmittedAction { get; set; }
    public DateTime CreatedAt { get; set; }

    public FormApplication? Application { get; set; }
}

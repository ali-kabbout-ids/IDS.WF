namespace Rwd.WF.Domain.Entities;

public sealed class SubWorkflowResult
{
    public Guid Id { get; set; }
    public string ParentInstanceId { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

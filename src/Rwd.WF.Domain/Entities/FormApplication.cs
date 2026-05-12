namespace Rwd.WF.Domain.Entities;

/// <summary>
/// Persisted application payload (table: Applications).
/// </summary>
public sealed class FormApplication
{
    public Guid Id { get; set; }
    public string FormData { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
}

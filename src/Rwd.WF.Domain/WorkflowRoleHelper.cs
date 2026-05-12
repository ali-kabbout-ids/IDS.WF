using System.Globalization;

namespace Rwd.WF.Domain;

public static class WorkflowRoleHelper
{
    /// <summary>
    /// Maps a username or role token to a workflow role, or null if unknown.
    /// </summary>
    public static WorkflowRole? Parse(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var n = username.Trim();
        if (n.Equals("moaawen_shooba", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("MoaawenShooba", StringComparison.OrdinalIgnoreCase) ||
            n.Equals(nameof(WorkflowRole.MoaawenShooba), StringComparison.OrdinalIgnoreCase))
            return WorkflowRole.MoaawenShooba;

        if (n.Equals("iilam_kanouny", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("IilamKanouny", StringComparison.OrdinalIgnoreCase) ||
            n.Equals(nameof(WorkflowRole.IilamKanouny), StringComparison.OrdinalIgnoreCase))
            return WorkflowRole.IilamKanouny;

        return null;
    }

    /// <summary>
    /// Canonical role string stored in workflow payloads and <see cref="WorkflowTask.RequiredRole"/>.
    /// </summary>
    public static string ToRoleString(WorkflowRole role) =>
        role switch
        {
            WorkflowRole.MoaawenShooba => nameof(WorkflowRole.MoaawenShooba),
            WorkflowRole.IilamKanouny => nameof(WorkflowRole.IilamKanouny),
            _ => role.ToString()
        };
}

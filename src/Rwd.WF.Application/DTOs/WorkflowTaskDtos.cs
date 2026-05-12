namespace Rwd.WF.Application.DTOs;

public sealed record TaskSummaryDto(
    Guid TaskId,
    Guid ApplicationId,
    string StepName,
    string FormKey,
    string Status,
    string? ClaimedBy);

public sealed record TaskDetailDto(
    Guid TaskId,
    Guid ApplicationId,
    string StepName,
    string FormKey,
    IReadOnlyList<string> AvailableActions,
    object? ApplicationData,
    string Status);

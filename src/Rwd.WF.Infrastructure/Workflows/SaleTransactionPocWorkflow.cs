using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Activities;
using Rwd.WF.Infrastructure.ElsaActivities;

namespace Rwd.WF.Infrastructure.Workflows;

/// <summary>
/// Code-first definition for <c>sale-transaction-poc</c> (avoids Studio JSON import schema issues).
/// </summary>
public class SaleTransactionPocWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder workflow)
    {
        workflow.WithDefinitionId("sale-transaction-poc");
        workflow.Name = "Sale Transaction POC";
        workflow.Description = "POC workflow for sale transaction approval";

        var stepTake = UserTask(
            "step-take",
            "MoaawenShooba Takes Application",
            "MoaawenShooba",
            "Take",
            "sale-transaction-v1",
            "take_application");

        var stepIilam = UserTask(
            "step-iilam",
            "Iilam Kanouny Reviews",
            "IilamKanouny",
            "Submit",
            "sale-transaction-v1",
            "legal_review");

        var stepApprove = UserTask(
            "step-approve",
            "Moaawen Shooba Approves",
            "MoaawenShooba",
            "Approve,Reject",
            "sale-transaction-v1",
            "initial_approval");

        var checkApproved = new FlowDecision(
            ctx => string.Equals(
                stepApprove.GetOutput<string>(ctx, nameof(GenericUserTaskActivity.SelectedAction)),
                "Reject",
                StringComparison.OrdinalIgnoreCase),
            "check-approved");

        var stepMokhatabat = UserTask(
            "step-mokhatabat",
            "Need Mokhatabat?",
            "MoaawenShooba",
            "Yes,No",
            "sale-transaction-v1",
            "mokhatabat_check");

        var checkMokhatabat = new FlowDecision(
            ctx => string.Equals(
                stepMokhatabat.GetOutput<string>(ctx, nameof(GenericUserTaskActivity.SelectedAction)),
                "Yes",
                StringComparison.OrdinalIgnoreCase),
            "check-mokhatabat");

        var dispatchSub = new DispatchWorkflow("dispatch-sub")
        {
            WorkflowDefinitionId = new Input<string>(new Literal<string>("mokhatabat-sub-workflow")),
            WaitForCompletion = new Input<bool>(new Literal<bool>(true))
        };

        var stepMane3 = UserTask(
            "step-mane3",
            "Has Mane3 Kanone?",
            "MoaawenShooba",
            "Yes,No",
            "sale-transaction-v1",
            "mane3_check");

        var checkMane3 = new FlowDecision(
            ctx => string.Equals(
                stepMane3.GetOutput<string>(ctx, nameof(GenericUserTaskActivity.SelectedAction)),
                "Yes",
                StringComparison.OrdinalIgnoreCase),
            "check-mane3");

        var httpMane3 = new SendHttpRequest("http-mane3")
        {
            Url = new Input<Uri?>(new Literal<Uri>(new Uri("http://localhost:5000/api/check/mane3kanone/00000000-0000-0000-0000-000000000000"))),
            Method = new Input<string>(new Literal<string>("POST")),
            ContentType = new Input<string?>(new Literal<string>("application/json"))
        };

        // Elsa Studio flowchart mapper reads metadata.designer.position (and size); without it, nodes stack at (0,0).
        SetDesignerLayout(stepTake, 40, 100);
        SetDesignerLayout(stepIilam, 340, 100);
        SetDesignerLayout(stepApprove, 640, 100);
        SetDesignerLayout(checkApproved, 940, 100);
        SetDesignerLayout(stepMokhatabat, 940, 280);
        SetDesignerLayout(checkMokhatabat, 1240, 280);
        SetDesignerLayout(dispatchSub, 1540, 200);
        SetDesignerLayout(stepMane3, 1240, 440);
        SetDesignerLayout(checkMane3, 1540, 440);
        SetDesignerLayout(httpMane3, 1840, 440);

        // Studio 3 visual designer only renders when the workflow root is Elsa.Flowchart (not Sequence).
        workflow.Root = new Flowchart("root-flow")
        {
            Start = stepTake,
            Activities =
            {
                stepTake,
                stepIilam,
                stepApprove,
                checkApproved,
                stepMokhatabat,
                checkMokhatabat,
                dispatchSub,
                stepMane3,
                checkMane3,
                httpMane3
            },
            Connections =
            {
                new Connection(stepTake, stepIilam),
                new Connection(stepIilam, stepApprove),
                new Connection(stepApprove, checkApproved),
                new Connection(new Endpoint(checkApproved, "False"), new Endpoint(stepMokhatabat, default)),
                new Connection(new Endpoint(checkApproved, "True"), new Endpoint(stepIilam, default)),
                new Connection(stepMokhatabat, checkMokhatabat),
                new Connection(new Endpoint(checkMokhatabat, "True"), new Endpoint(dispatchSub, default)),
                new Connection(new Endpoint(checkMokhatabat, "False"), new Endpoint(stepMane3, default)),
                new Connection(dispatchSub, stepMane3),
                new Connection(stepMane3, checkMane3),
                new Connection(new Endpoint(checkMane3, "True"), new Endpoint(httpMane3, default))
            }
        };
    }

    /// <summary>
    /// Matches Elsa Studio / <see cref="Elsa.Api.Client.Shared.Models.ActivityDesignerMetadata"/> JSON shape under <c>metadata.designer</c>.
    /// </summary>
    private static void SetDesignerLayout(IActivity activity, double x, double y, double width = 220, double height = 72)
    {
        activity.Metadata["designer"] = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["position"] = new Dictionary<string, object>(StringComparer.Ordinal) { ["x"] = x, ["y"] = y },
            ["size"] = new Dictionary<string, object>(StringComparer.Ordinal) { ["width"] = width, ["height"] = height }
        };
    }

    private static GenericUserTaskActivity UserTask(
        string id,
        string displayName,
        string requiredRole,
        string availableActions,
        string formKey,
        string stepName) =>
        new()
        {
            Id = id,
            Name = displayName,
            RequiredRole = new Input<string>(new Literal<string>(requiredRole)),
            AvailableActions = new Input<string>(new Literal<string>(availableActions)),
            FormKey = new Input<string>(new Literal<string>(formKey)),
            StepName = new Input<string>(new Literal<string>(stepName))
        };
}

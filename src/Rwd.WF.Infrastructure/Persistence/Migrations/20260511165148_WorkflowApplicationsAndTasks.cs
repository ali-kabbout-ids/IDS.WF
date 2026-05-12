using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rwd.WF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowApplicationsAndTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormData = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubWorkflowResults",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentInstanceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubWorkflowResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElsaTaskId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AvailableActions = table.Column<string>(type: "jsonb", nullable: false),
                    FormKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StepName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClaimedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAction = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "workflow",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubWorkflowResults_ParentInstanceId",
                schema: "workflow",
                table: "SubWorkflowResults",
                column: "ParentInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ApplicationId_Status",
                schema: "workflow",
                table: "WorkflowTasks",
                columns: new[] { "ApplicationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ElsaTaskId",
                schema: "workflow",
                table: "WorkflowTasks",
                column: "ElsaTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubWorkflowResults",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "WorkflowTasks",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "workflow");
        }
    }
}

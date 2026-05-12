using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence.Configurations;

public sealed class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("WorkflowTasks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ElsaTaskId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.RequiredRole).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AvailableActions).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.FormKey).IsRequired().HasMaxLength(200);
        builder.Property(x => x.StepName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ClaimedBy).HasMaxLength(200);
        builder.Property(x => x.SubmittedAction).HasMaxLength(200);

        builder.Property(x => x.ClaimedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.ElsaTaskId);
        builder.HasIndex(x => new { x.ApplicationId, x.Status });
    }
}

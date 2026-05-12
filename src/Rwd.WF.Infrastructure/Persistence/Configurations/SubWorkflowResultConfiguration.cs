using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence.Configurations;

public sealed class SubWorkflowResultConfiguration : IEntityTypeConfiguration<SubWorkflowResult>
{
    public void Configure(EntityTypeBuilder<SubWorkflowResult> builder)
    {
        builder.ToTable("SubWorkflowResults");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParentInstanceId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Result).IsRequired();
        builder.Property(x => x.CompletedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.ParentInstanceId);
    }
}

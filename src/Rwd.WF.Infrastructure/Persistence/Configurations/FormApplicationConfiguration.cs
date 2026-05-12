using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence.Configurations;

public sealed class FormApplicationConfiguration : IEntityTypeConfiguration<FormApplication>
{
    public void Configure(EntityTypeBuilder<FormApplication> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FormData)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(x => x.Status).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasMany(x => x.Tasks)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

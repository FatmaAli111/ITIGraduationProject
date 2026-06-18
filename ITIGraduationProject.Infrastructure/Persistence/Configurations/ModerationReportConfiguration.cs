using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class ModerationReportConfiguration : IEntityTypeConfiguration<ModerationReport>
{
    public void Configure(EntityTypeBuilder<ModerationReport> builder)
    {
        builder.ToTable("ModerationReports");

        builder.HasKey(mr => mr.Id);
        builder.Property(mr => mr.CreatedAt).IsRequired();
        builder.Property(mr => mr.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(mr => !mr.IsDeleted);
        builder.Property(mr => mr.Status).HasColumnType("int");


        builder.Property(mr => mr.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(mr => mr.ActionTaken).HasMaxLength(1000);
        

    }
}

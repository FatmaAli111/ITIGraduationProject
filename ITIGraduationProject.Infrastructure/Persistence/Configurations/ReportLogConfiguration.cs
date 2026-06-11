using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class ReportLogConfiguration : IEntityTypeConfiguration<ReportLog>
{
    public void Configure(EntityTypeBuilder<ReportLog> builder)
    {
        builder.ToTable("ReportLogs");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.Property(r => r.ReportType).IsRequired().HasMaxLength(200);
        builder.Property(r => r.FileURL).HasMaxLength(1000);
        builder.Property(r => r.Filters).HasMaxLength(2000);
        builder.Property(r => r.AdminId).IsRequired();
    }
}

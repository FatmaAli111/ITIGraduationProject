using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class PrinterProfileConfiguration : IEntityTypeConfiguration<PrinterProfile>
{
    public void Configure(EntityTypeBuilder<PrinterProfile> builder)
    {
        builder.ToTable("PrinterProfiles");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.SupportedFabrics).IsRequired().HasColumnType("int");
        builder.Property(p => p.SupportedPrintMethods).IsRequired().HasColumnType("int");
        builder.Property(p => p.UserId).IsRequired();


        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.PrinterProfile)
            .HasForeignKey(oi => oi.PrinterProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

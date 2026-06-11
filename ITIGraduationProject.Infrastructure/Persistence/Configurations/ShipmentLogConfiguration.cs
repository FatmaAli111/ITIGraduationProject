using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class ShipmentLogConfiguration : IEntityTypeConfiguration<ShipmentLog>
{
    public void Configure(EntityTypeBuilder<ShipmentLog> builder)
    {
        builder.ToTable("ShipmentLogs");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Notes).HasMaxLength(1000);
        builder.Property(sl => sl.ShipmentId).IsRequired();
        builder.Property(sl => sl.Status).HasColumnType("int");
        builder.Property(sl => sl.ChangedAt).HasColumnType("datetime").IsRequired();
 
    }
}

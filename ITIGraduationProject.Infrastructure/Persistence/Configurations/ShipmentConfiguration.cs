using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.Property(s => s.Status).HasColumnType("int");
        builder.Property(s => s.Provider).HasMaxLength(100);
        builder.Property(s => s.EstimatedDeliveryDate).IsRequired();
        builder.Property(s => s.ShippedAt);
        builder.Property(s => s.DeliveredAt);
        builder.Property(s => s.TrackingNumber).HasMaxLength(200);
        builder.Property(s => s.OrderId).IsRequired();

        builder.HasMany(s => s.ShipmentLogs)
            .WithOne(sl => sl.Shipment)
            .HasForeignKey(sl => sl.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

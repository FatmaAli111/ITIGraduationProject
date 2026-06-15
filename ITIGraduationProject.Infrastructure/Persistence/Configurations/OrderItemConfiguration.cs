using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.SnapshotImageURL).HasMaxLength(500);
        builder.Property(oi => oi.PriceBreakdown).HasMaxLength(1000);
        builder.Property(oi => oi.Status).HasColumnType("int");
        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.OrderId).IsRequired();
        builder.Property(oi => oi.DesignId).IsRequired();

        
    }
}

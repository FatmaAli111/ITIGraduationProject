using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.Property(o => o.ReceiverName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.PhoneNumber).HasMaxLength(50);
        builder.Property(o => o.Address).HasMaxLength(1000);
        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.OrderStatus).HasColumnType("int");
        builder.Property(o => o.PaymentStatus).HasColumnType("int");
        builder.Property(o => o.PointsRedeemed).HasDefaultValue(0);
        builder.Property(o => o.DeliveryNotes).HasMaxLength(2000);
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.CouponId).IsRequired(false);
        builder.Property(o => o.RewardId).IsRequired(false);
        

        builder.HasOne(o => o.Reward)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.RewardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Shipment)
            .WithOne(s => s.Order)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

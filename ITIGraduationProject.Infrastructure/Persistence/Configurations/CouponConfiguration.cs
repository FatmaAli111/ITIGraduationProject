using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.Property(c => c.DiscountType).HasColumnType("int");
        builder.Property(c => c.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.ExpiryDate).IsRequired();
        builder.Property(c => c.UsageLimit).IsRequired();
        builder.Property(c => c.UsedCount).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.DiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Coupon)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

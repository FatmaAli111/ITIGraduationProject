using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.SnapshotImageUrl).HasMaxLength(500);
        builder.Property(ci => ci.Quantity).HasDefaultValue(1);
        builder.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(ci => ci.CreatedAt).IsRequired();

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Design)
            .WithMany(d => d.CartItems)
            .HasForeignKey(ci => ci.DesignId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


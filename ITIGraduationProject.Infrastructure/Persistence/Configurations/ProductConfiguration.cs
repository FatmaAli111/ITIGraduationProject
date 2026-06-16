using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Products;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AvailableColors).HasColumnType("int");
        builder.Property(p => p.PreviewImageURL).HasMaxLength(500);
        builder.Property(p => p.StockStatus).HasMaxLength(100);
        builder.Property(p => p.AverageRating).HasColumnType("decimal(3,2)");
        


        builder.HasMany(p => p.Designs)
            .WithOne(d => d.Product)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ProductImages)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

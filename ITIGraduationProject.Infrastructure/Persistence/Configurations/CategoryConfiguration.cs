using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Products;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.Property(c => c.PrintableAreaConfig).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.ImageFileName).HasMaxLength(255);
        builder.Property(c => c.Description).HasMaxLength(1000);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Templates)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

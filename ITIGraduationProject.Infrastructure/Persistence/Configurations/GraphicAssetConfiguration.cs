using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class GraphicAssetConfiguration : IEntityTypeConfiguration<GraphicAsset>
{
    public void Configure(EntityTypeBuilder<GraphicAsset> builder)
    {
        builder.ToTable("GraphicAssets");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(g => !g.IsDeleted);

        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
        builder.Property(g => g.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(g => g.Tags).HasMaxLength(500);
        builder.Property(g => g.Type).IsRequired();
        builder.Property(g => g.UserId).IsRequired();

    }
}

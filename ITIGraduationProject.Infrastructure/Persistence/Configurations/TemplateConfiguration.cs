using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Products;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.PreviewImageURL).HasMaxLength(500);
        builder.Property(t => t.StyleTags).HasMaxLength(500);
        builder.Property(t => t.AverageRating).HasColumnType("decimal(3,2)");
        builder.Property(t => t.LikesCount).HasDefaultValue(0);
        builder.Property(t => t.RemixesCount).HasDefaultValue(0);
        builder.Property(t => t.ReviewCount).HasDefaultValue(0);
        builder.Property(t => t.IsPublic).HasDefaultValue(false);
        builder.Property(t => t.CategoryId).IsRequired();
        builder.Property(t => t.CreatorUserId).IsRequired();
        

        builder.HasMany(t => t.Designs)
            .WithOne(d => d.Template)
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Rewards)
            .WithOne(r => r.Template)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.ModerationReports)
            .WithOne(m => m.TargetTemplate)
            .HasForeignKey(m => m.TargetTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.CommunityInteractions)
               .WithOne(ci => ci.Template)
               .HasForeignKey(ci => ci.TemplateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

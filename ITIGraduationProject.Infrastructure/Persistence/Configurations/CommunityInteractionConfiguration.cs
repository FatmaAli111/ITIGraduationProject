using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class CommunityInteractionConfiguration : IEntityTypeConfiguration<CommunityInteraction>
{
    public void Configure(EntityTypeBuilder<CommunityInteraction> builder)
    {
        builder.ToTable("CommunityInteractions");

        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.CreatedAt).IsRequired();
        builder.Property(ci => ci.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(ci => !ci.IsDeleted);
        builder.Property(ci => ci.Content).HasMaxLength(1000);
        builder.Property(ci => ci.InteractionType).HasColumnType("int");
        builder.Property(ci => ci.UserId).IsRequired();
        builder.Property(ci => ci.TemplateId).IsRequired();

    }
}

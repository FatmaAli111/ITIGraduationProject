using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.HasKey(up => up.Id);

        builder.Property(up => up.CreatedAt).IsRequired();
        builder.Property(up => up.UpdatedAt);
        builder.Property(up => up.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(up => !up.IsDeleted);

        builder.Property(up => up.FavoriteColors).HasMaxLength(500);
        builder.Property(up => up.BannedColors).HasMaxLength(500);
        builder.Property(up => up.StyleType).HasMaxLength(100);
        builder.Property(up => up.Interests).HasMaxLength(500);
        builder.Property(up => up.DesignPreference).HasMaxLength(200);
        builder.Property(up => up.ContentPreference).HasMaxLength(200);
        builder.Property(up => up.IssuedBadges).HasMaxLength(1000);
    }
}

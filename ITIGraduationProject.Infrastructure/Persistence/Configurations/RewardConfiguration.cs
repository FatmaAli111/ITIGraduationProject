using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("Rewards");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.Property(r => r.RewardValue).HasColumnType("decimal(18,2)");
        builder.Property(r => r.BadgeImageUrl).HasMaxLength(500);
        builder.Property(r => r.RewardType).HasColumnType("int");
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.RewardRuleId).IsRequired();
        builder.Property(r => r.TemplateId).IsRequired(false);
        builder.Property(r => r.IsClaimed).HasDefaultValue(false);

        builder.HasOne(r => r.RewardRule)
            .WithMany(rr => rr.Rewards)
            .HasForeignKey(r => r.RewardRuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

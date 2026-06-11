using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class RewardRuleConfiguration : IEntityTypeConfiguration<RewardRule>
{
    public void Configure(EntityTypeBuilder<RewardRule> builder)
    {
        builder.ToTable("RewardRules");

        builder.HasKey(rr => rr.Id);
        builder.Property(rr => rr.CreatedAt).IsRequired();
        builder.Property(rr => rr.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(rr => !rr.IsDeleted);

        builder.Property(rr => rr.RewardValue).HasColumnType("decimal(18,2)");
        builder.Property(rr => rr.IsActive).HasDefaultValue(true);
        builder.Property(rr => rr.LikesThreshold).HasDefaultValue(0);
        builder.Property(rr => rr.SavesThreshold).HasDefaultValue(0);
        builder.Property(rr => rr.RewardType).HasColumnType("int");
       
    }
}

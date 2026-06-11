using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class AiChatSessionConfiguration : IEntityTypeConfiguration<AiChatSession>
{
    public void Configure(EntityTypeBuilder<AiChatSession> builder)
    {
        builder.ToTable("AiChatSessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.Property(s => s.SessionType).HasColumnType("int").IsRequired();

        builder.HasMany(s => s.AiChatMessages)
               .WithOne(m => m.AiChatSession)
               .HasForeignKey(m => m.AiChatSessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

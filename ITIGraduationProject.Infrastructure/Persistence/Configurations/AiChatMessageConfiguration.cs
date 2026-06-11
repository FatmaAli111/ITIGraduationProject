using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("AiChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Sender).IsRequired().HasMaxLength(100);
        builder.Property(m => m.MessageText).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
        builder.Property(m => m.Sender).IsRequired().HasMaxLength(50);
        builder.Property(m => m.MessageText).IsRequired();

    }
}

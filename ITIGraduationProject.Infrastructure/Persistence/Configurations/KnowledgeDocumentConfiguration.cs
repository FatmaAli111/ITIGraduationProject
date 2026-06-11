using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("KnowledgeDocuments");

        builder.HasKey(k => k.Id);
        builder.Property(k => k.CreatedAt).IsRequired();
        builder.Property(k => k.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(k => !k.IsDeleted);
        builder.Property(k => k.CreatedByAdminId).IsRequired();
        builder.Property(k => k.IsEmbedded).HasDefaultValue(false);
        builder.Property(k => k.Tags).HasMaxLength(1000);

        builder.Property(k => k.Topic).IsRequired().HasMaxLength(500);
        builder.Property(k => k.SourceText).IsRequired();
    }
}

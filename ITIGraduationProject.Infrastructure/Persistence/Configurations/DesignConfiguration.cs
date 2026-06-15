using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class DesignConfiguration : IEntityTypeConfiguration<Design>
{
    public void Configure(EntityTypeBuilder<Design> builder)
    {
        builder.ToTable("Designs");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.Property(d => d.CanvasStateJSON).IsRequired();
        builder.Property(d => d.SnapshotImageURL).HasMaxLength(500);
        builder.Property(d => d.CalculatedPrice).HasColumnType("decimal(18,2)");
        builder.Property(d => d.SelectedPrintMethod).HasColumnType("int");
        builder.Property(d => d.SelectedFabric).HasColumnType("int");
        builder.Property(d => d.SelectedSize).HasColumnType("int");
        builder.Property(d => d.Status).HasColumnType("int");
        builder.Property(d => d.SelectedColor).HasMaxLength(50);



        builder.HasMany(d => d.CartItems)
            .WithOne(ci => ci.Design)
            .HasForeignKey(ci => ci.DesignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.OrderItems)
            .WithOne(oi => oi.Design)
            .HasForeignKey(oi => oi.DesignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.AiChatSessions)
            .WithOne(s => s.CurrentDesign)
            .HasForeignKey(s => s.CurrentDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.GraphicAssets)
               .WithMany(ga => ga.Designs)
               .UsingEntity(j => j.ToTable("DesignGraphicAssets"));

        builder.HasMany(d => d.DesignImages)
            .WithOne(di => di.Design)
            .HasForeignKey(di => di.DesignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

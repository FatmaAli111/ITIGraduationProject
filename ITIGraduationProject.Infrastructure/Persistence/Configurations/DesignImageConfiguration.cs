using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class DesignImageConfiguration : IEntityTypeConfiguration<DesignImage>
{
    public void Configure(EntityTypeBuilder<DesignImage> builder)
    {
        builder.ToTable("DesignImages");

        builder.HasKey(di => di.Id);

        builder.Property(di => di.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(di => di.IsPrimary).IsRequired();
        builder.Property(di => di.DesignId).IsRequired();
        


    }
}

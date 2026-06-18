using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Infrastructure.Identity;
namespace ITIGraduationProject.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.DeletedAt);
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(500);
        builder.Property(u => u.CurrentPointsBalance).HasDefaultValue(0);
        builder.Property(u => u.IsActive).HasDefaultValue(true);

        
        builder.HasOne(u => u.UserPreferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.PrinterProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PrinterProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

       
        builder.HasMany(u => u.Designs)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Templates)
            .WithOne(t => t.CreatorUser)
            .HasForeignKey(t => t.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AiChatSessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.CommunityInteractions)
            .WithOne(ci => ci.User)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Rewards)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ModerationReports)
            .WithOne(m => m.ReporterUser)
            .HasForeignKey(m => m.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<ApplicationUser>(au => au.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

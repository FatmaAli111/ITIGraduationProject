using System;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Infrastructure.Identity;

public static class ApplicationSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ApplicationSeeder");

        await SeedCommunityAsync(context, logger);
    }

    private static async Task SeedCommunityAsync(AppDbContext context, ILogger logger)
    {
        await SeedTemplatesAsync(context, logger);
    }

    private static async Task SeedTemplatesAsync(AppDbContext context, ILogger logger)
    {
        var creatorId = await context.Users
     .Where(x => x.Email == "FatmaAli@gmail.com")
     .Select(x => x.Id)
     .FirstOrDefaultAsync();
        if (creatorId == Guid.Empty)
        {
            logger.LogWarning("No existing users found. Skipping template seeding.");
            return;
        }

        var categoryId = await GetOrCreateDevCategoryIdAsync(context, logger);
        if (categoryId == Guid.Empty)
        {
            logger.LogWarning("Unable to resolve a category for template seeding.");
            return;
        }

        var hasUnpublished = await context.Templates.AnyAsync(t => t.CreatorUserId == creatorId && t.Name == "Cyberpunk Retro Wave");
        if (!hasUnpublished)
        {
            var unpublishedTemplates = new[]
            {
                new Template
                {
                    CategoryId = categoryId,
                    CreatorUserId = creatorId,
                    Name = "Cyberpunk Retro Wave",
                    StyleTags = "cyberpunk, retro, neon",
                    PreviewImageURL = "https://placehold.co/600x600?text=Cyberpunk+Retro",
                    IsPublic = false,
                    LikesCount = 0,
                    ReviewCount = 0,
                    AverageRating = 0m,
                    RemixesCount = 0,
                    CanvasStateJSON = "{}"
                },
                new Template
                {
                    CategoryId = categoryId,
                    CreatorUserId = creatorId,
                    Name = "Minimalist Botanical",
                    StyleTags = "minimalist, botanical, floral",
                    PreviewImageURL = "https://placehold.co/600x600?text=Minimalist+Botanical",
                    IsPublic = false,
                    LikesCount = 0,
                    ReviewCount = 0,
                    AverageRating = 0m,
                    RemixesCount = 0,
                    CanvasStateJSON = "{}"
                },
                new Template
                {
                    CategoryId = categoryId,
                    CreatorUserId = creatorId,
                    Name = "Abstract Geometry Draft",
                    StyleTags = "abstract, geometric, art",
                    PreviewImageURL = "https://placehold.co/600x600?text=Abstract+Geometry",
                    IsPublic = false,
                    LikesCount = 0,
                    ReviewCount = 0,
                    AverageRating = 0m,
                    RemixesCount = 0,
                    CanvasStateJSON = "{}"
                },
                new Template
                {
                    CategoryId = categoryId,
                    CreatorUserId = creatorId,
                    Name = "Vintage Typography",
                    StyleTags = "vintage, typography, custom",
                    PreviewImageURL = "https://placehold.co/600x600?text=Vintage+Typography",
                    IsPublic = false,
                    LikesCount = 0,
                    ReviewCount = 0,
                    AverageRating = 0m,
                    RemixesCount = 0,
                    CanvasStateJSON = "{}"
                }
            };

            context.Templates.AddRange(unpublishedTemplates);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded unpublished templates for development.");
        }

        if (await context.Templates.AnyAsync(t => t.IsPublic && !t.IsDeleted))
        {
            logger.LogInformation("Public templates table is not empty. Skipping public templates seeding.");
            return;
        }

        var templates = new[]
        {
            new Template
            {
                CategoryId = categoryId,
                CreatorUserId = creatorId,
                Name = "Coastal Horizon",
                StyleTags = "minimal, nature, sunset",
                PreviewImageURL = "https://placehold.co/600x600?text=Coastal+Horizon",
                IsPublic = true,
                LikesCount = 12,
                ReviewCount = 3,
                AverageRating = 4.50m,
                RemixesCount = 2,
            },
            new Template
            {
                CategoryId = categoryId,
                CreatorUserId = creatorId,
                Name = "Streetwear Monogram Draft",
                StyleTags = "typography, urban",
                PreviewImageURL = "https://placehold.co/600x600?text=Streetwear+Monogram",
                IsPublic = false,
                LikesCount = 0,
                ReviewCount = 0,
                AverageRating = 0m,
                RemixesCount = 0,
            },
        };

        context.Templates.AddRange(templates);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} development templates.", templates.Length);
    }

    private static async Task<Guid> GetOrCreateDevCategoryIdAsync(AppDbContext context, ILogger logger)
    {
        var existingCategoryId = await context.Categories
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (existingCategoryId != Guid.Empty)
            return existingCategoryId;

        var category = new Category
        {
            Name = "T-Shirts",
            Description = "Classic cotton tees for community designs.",
            PrintableAreaConfig = "{}",
            ImageUrl = "https://placehold.co/400x400?text=T-Shirts",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded development category '{CategoryName}'.", category.Name);

        return category.Id;
    }
}

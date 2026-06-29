using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using ITIGraduationProject.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Infrastructure.Identity;

/// <summary>
/// Development-only, deterministic demo data for the graduation presentation.
/// Fixed IDs make every stage safe to run again after a restart or partial seed.
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Demo@123456";

    private static readonly string[] ProductImagePaths =
    {
        "/uploads/Products/2fc5d29f-1fb3-4ae9-994c-18aa03858888.png",
        "/uploads/Products/3edab8e4-4558-4389-980a-6a7646ce1b17.png",
        "/uploads/Products/66e7c949-562f-4a28-b216-0406a8758a9f.png",
        "/uploads/Products/7076e954-0f35-450f-9629-4e4d1d814c4b.png",
        "/uploads/Products/bb7ebb9d-4807-4322-9659-43ac9bc888ee.png",
        "/uploads/Products/c4f86d95-2f05-42a2-8722-46ed4528b60d.png"
    };

    private static readonly string[] TemplateImagePaths =
    {
        "/uploads/templates/23d1e54f-4c5f-4502-afb7-8779ac54caea.png",
        "/uploads/templates/7eb1aeac-3fa4-4704-b1ea-0dde939d6af7.png",
        "/uploads/templates/bd9e4e20-52d7-4b37-8040-cbb902500c0f.png",
        "/uploads/templates/eb6f5442-b77f-4e01-b901-03e98ff233ae.png",
        "/uploads/designs/4896dc93-c26c-4d29-a31d-aa1b86e77914.png",
        "/uploads/designs/78696cf4-37fa-4a5c-9699-ecdf5252d752.png",
        "/uploads/designs/b73e7c41-82bb-49e4-a8d6-e427375f2eee.png",
        "/uploads/designs/d7402956-1059-4b88-9644-af6681ddcb28.png",
        "/uploads/designs/d97f22c9-6e05-4842-bf65-deb61d006f3d.png",
        "/uploads/designs/f3899b70-8f05-400b-8d72-6229b0525ac6.png",
        "/uploads/designs/fa63eeb1-9933-4383-84af-2d6d82305783.png"
    };

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DemoDataSeeder");

        try
        {
            logger.LogInformation("Demo data seeding started.");

            var userIds = await SeedUsersAsync(context, userManager, logger);
            var printerProfileId = await EnsurePrinterProfileAsync(context, userManager, configuration, logger);
            var categoryIds = await SeedCategoriesAsync(context, logger);
            var productIds = await SeedProductsAsync(context, categoryIds, logger);
            await SeedProductImagesAsync(context, productIds, logger);
            await EnsureProductForEveryCategoryAsync(context, logger);
            var templateIds = await SeedTemplatesAsync(context, categoryIds, userIds, logger);
            await SeedCommunityInteractionsAsync(context, templateIds, userIds, logger);
            var designIds = await SeedDesignsAsync(context, productIds, templateIds, userIds, logger);
            var orderIds = await SeedOrdersAsync(context, userIds, logger);
            await SeedOrderItemsAsync(context, orderIds, designIds, printerProfileId, logger);
            await SeedModerationReportsAsync(context, templateIds, userIds, logger);

            logger.LogInformation("Demo data seeding completed.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Demo data seeding failed.");
            throw;
        }
    }

    private static async Task<IReadOnlyList<Guid>> SeedUsersAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var seeds = new[]
        {
            new DemoUser("Maya Okafor", "demo.maya@wearly.local", "Pattern artist exploring bold color and movement.", "Red, Yellow, Black", "Art, Fashion", "Bold Prints"),
            new DemoUser("Theo Wren", "demo.theo@wearly.local", "Typography-led streetwear designer based in London.", "Black, White, Green", "Music, Photography", "Clean & Simple"),
            new DemoUser("Lin Park", "demo.lin@wearly.local", "Minimal forms, soft palettes, and thoughtful details.", "White, Pastels, Neutral", "Art, Travel", "Clean & Simple"),
            new DemoUser("Sofia Renault", "demo.sofia@wearly.local", "Illustrator turning everyday moments into wearable stories.", "Blue, Red, White", "Travel, Fashion", "Mixed"),
            new DemoUser("Omar El-Sayed", "demo.omar@wearly.local", "Cairo creative working across type, sport, and culture.", "Black, Orange, Earth Tones", "Sports, Music", "Bold Prints"),
            new DemoUser("Nadia Bell", "demo.nadia@wearly.local", "Botanical studies and calm editorial graphics.", "Green, White, Earth Tones", "Photography, Art", "Clean & Simple"),
            new DemoUser("Jonas Meyer", "demo.jonas@wearly.local", "Experimental graphics inspired by architecture and sound.", "Blue, Black, Neutral", "Tech, Music", "Mixed"),
            new DemoUser("Amara Cole", "demo.amara@wearly.local", "Playful color, optimistic messages, and community design.", "Pink, Yellow, Purple", "Gaming, Fashion", "Bold Prints"),
            new DemoUser("Rami Haddad", "demo.rami@wearly.local", "Vintage sports references with a contemporary finish.", "Green, Red, Neutral", "Sports, Travel", "Mixed"),
            new DemoUser("Elena Rossi", "demo.elena@wearly.local", "Mediterranean color stories and hand-drawn lettering.", "Blue, White, Pastels", "Travel, Art", "Clean & Simple")
        };

        var userIds = new List<Guid>(seeds.Length);
        var createdCount = 0;

        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var identityUser = await userManager.FindByEmailAsync(seed.Email);
            var expectedId = SeedId(1, index + 1);
            var domainUser = identityUser == null
                ? await context.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(user => user.Id == expectedId)
                : await context.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(user => user.Id == identityUser.Id);
            var domainUserAdded = false;

            if (domainUser == null)
            {
                domainUser = new User
                {
                    Id = identityUser?.Id ?? expectedId,
                    Name = seed.Name,
                    Email = seed.Email,
                    UserName = seed.Email,
                    Bio = seed.Bio,
                    ProfileImageUrl = null,
                    IsActive = true,
                    OnboardingCompleted = true,
                    CurrentPointsBalance = 120 + (index * 35),
                    TotalRewardPoints = 360 + (index * 90),
                    CurrentRank = index + 1,
                    CreatedAt = now.AddDays(-(55 - (index * 4))),
                    UserPreferences = new UserPreferences
                    {
                        FavoriteColors = seed.Colors,
                        Interests = seed.Interests,
                        DesignPreference = seed.DesignPreference,
                        StyleMatchPercentage = 78 + (index % 5) * 4,
                        IssuedBadges = index % 2 == 0 ? "Early Creator, Community Favorite" : "Rising Designer"
                    },
                    Cart = new Cart()
                };

                context.AppUsers.Add(domainUser);
                await context.SaveChangesAsync();
                domainUserAdded = true;
            }

            if (identityUser == null)
            {
                identityUser = new ApplicationUser
                {
                    Id = domainUser.Id,
                    Email = seed.Email,
                    UserName = seed.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(identityUser, DemoPassword);
                if (!createResult.Succeeded)
                {
                    if (domainUserAdded)
                    {
                        context.AppUsers.Remove(domainUser);
                        await context.SaveChangesAsync();
                    }

                    throw new InvalidOperationException(
                        $"Unable to create demo user {seed.Email}: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
                }

                createdCount++;
            }
            else if (!identityUser.EmailConfirmed)
            {
                identityUser.EmailConfirmed = true;
                await userManager.UpdateAsync(identityUser);
            }

            if (!await userManager.IsInRoleAsync(identityUser, Roles.User))
            {
                await userManager.AddToRoleAsync(identityUser, Roles.User);
            }

            userIds.Add(domainUser.Id);
        }

        logger.LogInformation("Seeded {Count} demo user account(s); {Total} deterministic accounts are available.", createdCount, seeds.Length);
        return userIds;
    }

    private static async Task<Guid?> EnsurePrinterProfileAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var printerEmail = configuration["PrinterSeed:Email"];
        var printerUser = string.IsNullOrWhiteSpace(printerEmail)
            ? null
            : await userManager.FindByEmailAsync(printerEmail);

        if (printerUser == null)
        {
            var existingProfile = await context.PrinterProfiles.OrderBy(profile => profile.CreatedAt).FirstOrDefaultAsync();
            if (existingProfile == null)
            {
                throw new InvalidOperationException("Demo orders require at least one printer profile.");
            }

            logger.LogInformation("Using existing printer profile {PrinterProfileId} for demo order assignments.", existingProfile.Id);
            return existingProfile.Id;
        }

        var profile = await context.PrinterProfiles.FirstOrDefaultAsync(item => item.UserId == printerUser.Id);
        if (profile != null)
        {
            logger.LogInformation("Seeded 0 demo printer profile(s); using {PrinterProfileId}.", profile.Id);
            return profile.Id;
        }

        profile = new PrinterProfile
        {
            Id = SeedId(9, 1),
            UserId = printerUser.Id,
            SupportedFabrics = FabricType.Cotton | FabricType.Polyester | FabricType.Linen,
            SupportedPrintMethods = PrintMethodType.DirectToGarment | PrintMethodType.ScreenPrinting | PrintMethodType.Embroidery,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-48)
        };
        context.PrinterProfiles.Add(profile);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded 1 demo printer profile with ID {PrinterProfileId}.", profile.Id);
        return profile.Id;
    }

    private static async Task<IReadOnlyList<Guid>> SeedCategoriesAsync(AppDbContext context, ILogger logger)
    {
        var now = DateTime.UtcNow;
        var categories = new[]
        {
            ("Oversized Tees", "Relaxed heavyweight tees built for statement graphics."),
            ("Essential Tees", "Everyday cotton staples with a clean print surface."),
            ("Hoodies", "Premium fleece layers for front, back, and sleeve artwork."),
            ("Sweatshirts", "Soft crewnecks for understated seasonal designs."),
            ("Tote Bags", "Durable canvas totes for bold portable artwork."),
            ("Caps", "Structured and relaxed caps ready for embroidery."),
            ("Jackets", "Light outerwear with versatile printable panels."),
            ("Kidswear", "Comfortable pieces designed for bright playful graphics."),
            ("Activewear", "Performance-ready garments for energetic designs."),
            ("Accessories", "Small-format pieces that complete a custom collection.")
        };

        var seeds = categories.Select((category, index) => new Category
        {
            Id = SeedId(2, index + 1),
            Name = category.Item1,
            Description = category.Item2,
            PrintableAreaConfig = "{\"x\":0.18,\"y\":0.14,\"width\":0.64,\"height\":0.68}",
            ImageUrl = ProductImagePaths[index % ProductImagePaths.Length],
            ImageFileName = Path.GetFileName(ProductImagePaths[index % ProductImagePaths.Length]),
            CreatedAt = now.AddDays(-(60 - index))
        }).ToArray();

        await AddMissingAsync(context, context.Categories, seeds, "categories", logger);
        return seeds.Select(seed => seed.Id).ToArray();
    }

    private static async Task<IReadOnlyList<Guid>> SeedProductsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> categoryIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var products = new[]
        {
            new ProductSeed("Studio Heavyweight Hoodie", 2, 58.00m, ProductAvailableColors.White | ProductAvailableColors.Gray, 4.82m, 184, "In stock"),
            new ProductSeed("Cairo Nights Oversized Tee", 0, 38.00m, ProductAvailableColors.Black | ProductAvailableColors.Maroon | ProductAvailableColors.Blue, 4.71m, 96, "In stock"),
            new ProductSeed("Atelier Pullover Hoodie", 2, 62.00m, ProductAvailableColors.Black | ProductAvailableColors.Gray, 4.68m, 88, "In stock")
        };

        var previewImages = new[]
        {
            ProductImagePaths[4], // White hoodie front
            ProductImagePaths[5], // White tee front
            ProductImagePaths[2]  // Black hoodie front
        };

        var seeds = products.Select((product, index) => new Product
        {
            Id = SeedId(3, index + 1),
            CategoryId = categoryIds[product.CategoryIndex],
            Name = product.Name,
            BasePrice = product.Price,
            AvailableColors = product.Colors,
            PreviewImageURL = previewImages[index],
            IsAvailable = true,
            StockStatus = product.StockStatus,
            AverageRating = product.Rating,
            ReviewCount = product.Reviews,
            CreatedAt = now.AddDays(-(35 - (index * 2)))
        }).ToArray();

        await AddMissingAsync(context, context.Products, seeds, "products", logger);

        var seedIds = seeds.Select(seed => seed.Id).ToArray();
        var existingProducts = await context.Products.IgnoreQueryFilters()
            .Where(product => seedIds.Contains(product.Id))
            .ToListAsync();

        foreach (var product in existingProducts)
        {
            var seed = seeds.Single(item => item.Id == product.Id);
            product.CategoryId = seed.CategoryId;
            product.Name = seed.Name;
            product.BasePrice = seed.BasePrice;
            product.AvailableColors = seed.AvailableColors;
            product.PreviewImageURL = seed.PreviewImageURL;
            product.IsAvailable = true;
            product.StockStatus = seed.StockStatus;
            product.AverageRating = seed.AverageRating;
            product.ReviewCount = seed.ReviewCount;
            product.IsDeleted = false;
            product.DeletedAt = null;
        }

        await RepairLegacyProductRowsAsync(context, seedIds, logger);
        await context.SaveChangesAsync();
        return seeds.Select(seed => seed.Id).ToArray();
    }

    private static async Task RepairLegacyProductRowsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> canonicalProductIds,
        ILogger logger)
    {
        var originalProductIds = Enumerable.Range(1, 12)
            .Select(index => SeedId(3, index))
            .ToArray();
        var legacyProductIds = originalProductIds.Skip(3).ToArray();

        Guid CanonicalProductId(Guid productId)
        {
            var originalIndex = Array.IndexOf(originalProductIds, productId);
            return (originalIndex % ProductImagePaths.Length) switch
            {
                0 or 4 => canonicalProductIds[0],
                1 or 5 => canonicalProductIds[1],
                _ => canonicalProductIds[2]
            };
        }

        var designs = await context.Designs
            .Where(design => legacyProductIds.Contains(design.ProductId))
            .ToListAsync();
        foreach (var design in designs)
        {
            design.ProductId = CanonicalProductId(design.ProductId);
        }

        var cartItems = await context.CartItems
            .Where(item => legacyProductIds.Contains(item.ProductId))
            .ToListAsync();
        foreach (var item in cartItems)
        {
            item.ProductId = CanonicalProductId(item.ProductId);
        }

        var legacyProducts = await context.Products.IgnoreQueryFilters()
            .Where(product => legacyProductIds.Contains(product.Id))
            .ToListAsync();
        foreach (var product in legacyProducts)
        {
            product.IsDeleted = true;
            product.DeletedAt ??= DateTime.UtcNow;
        }

        logger.LogInformation(
            "Consolidated {ProductCount} legacy demo products; reassigned {DesignCount} designs and {CartItemCount} cart items.",
            legacyProducts.Count,
            designs.Count,
            cartItems.Count);
    }

    private static async Task SeedProductImagesAsync(
        AppDbContext context,
        IReadOnlyList<Guid> productIds,
        ILogger logger)
    {
        var printableZone = "{\"left\":0.2,\"top\":0.16,\"width\":0.6,\"height\":0.62}";
        var seeds = new[]
        {
            ProductImageSeed(SeedId(31, 1), productIds[0], ProductImagePaths[4], ProductAvailableColors.White, ViewAngle.Front, printableZone, true, 0),
            ProductImageSeed(SeedId(32, 1), productIds[0], ProductImagePaths[0], ProductAvailableColors.White, ViewAngle.Back, printableZone, false, 1),
            ProductImageSeed(SeedId(31, 2), productIds[1], ProductImagePaths[5], ProductAvailableColors.White, ViewAngle.Front, printableZone, true, 0),
            ProductImageSeed(SeedId(32, 2), productIds[1], ProductImagePaths[1], ProductAvailableColors.White, ViewAngle.Back, printableZone, false, 1),
            ProductImageSeed(SeedId(31, 3), productIds[2], ProductImagePaths[2], ProductAvailableColors.Black, ViewAngle.Front, printableZone, true, 0),
            ProductImageSeed(SeedId(32, 3), productIds[2], ProductImagePaths[3], ProductAvailableColors.Black, ViewAngle.Back, printableZone, false, 1)
        };

        var legacyImageIds = Enumerable.Range(4, 9)
            .Select(index => SeedId(31, index))
            .ToArray();
        var legacyImages = await context.ProductImages
            .Where(image => legacyImageIds.Contains(image.Id))
            .ToListAsync();
        if (legacyImages.Count > 0)
        {
            context.ProductImages.RemoveRange(legacyImages);
            await context.SaveChangesAsync();
        }

        await AddMissingAsync(context, context.ProductImages, seeds, "product images", logger);

        var seedImageIds = seeds.Select(seed => seed.Id).ToArray();
        var existingImages = await context.ProductImages
            .Where(image => seedImageIds.Contains(image.Id))
            .ToListAsync();
        foreach (var image in existingImages)
        {
            var seed = seeds.Single(item => item.Id == image.Id);
            image.ProductId = seed.ProductId;
            image.ImageUrl = seed.ImageUrl;
            image.Color = seed.Color;
            image.ViewAngle = seed.ViewAngle;
            image.PrintableZoneJson = seed.PrintableZoneJson;
            image.IsPrimary = seed.IsPrimary;
            image.DisplayOrder = seed.DisplayOrder;
        }

        await context.SaveChangesAsync();
    }

    private static ProductImage ProductImageSeed(
        Guid id,
        Guid productId,
        string imageUrl,
        ProductAvailableColors color,
        ViewAngle viewAngle,
        string printableZoneJson,
        bool isPrimary,
        int displayOrder) => new()
        {
            Id = id,
            ProductId = productId,
            ImageUrl = imageUrl,
            Color = color,
            ViewAngle = viewAngle,
            PrintableZoneJson = printableZoneJson,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder
        };

    private static async Task EnsureProductForEveryCategoryAsync(
        AppDbContext context,
        ILogger logger)
    {
        var catalog = new[]
        {
            new CategoryProductSeed("Essential Tees", "Classic Essential Tee", 24m, ProductAvailableColors.White | ProductAvailableColors.Black, "assets/garment-tshirt-white.jpg", 4.54m, 128),
            new CategoryProductSeed("Sweatshirts", "Heritage Crew Sweatshirt", 49m, ProductAvailableColors.White | ProductAvailableColors.Gray, "assets/garment-hoodie-cream.jpg", 4.61m, 82),
            new CategoryProductSeed("Tote Bags", "Everyday Canvas Tote", 19m, ProductAvailableColors.White | ProductAvailableColors.Brown, "assets/template-botanical.jpg", 4.38m, 71),
            new CategoryProductSeed("Caps", "Atelier Classic Cap", 27m, ProductAvailableColors.Black | ProductAvailableColors.White, "assets/garment-cap-black.jpg", 4.73m, 94),
            new CategoryProductSeed("Jackets", "Studio Utility Jacket", 84m, ProductAvailableColors.Black | ProductAvailableColors.Gray, "assets/garment-hoodie-charcoal.jpg", 4.42m, 53),
            new CategoryProductSeed("Kidswear", "Little Maker Tee", 21m, ProductAvailableColors.White | ProductAvailableColors.Blue | ProductAvailableColors.Pink, "assets/garment-tshirt-white.jpg", 4.66m, 61),
            new CategoryProductSeed("Activewear", "Motion Training Pants", 44m, ProductAvailableColors.Green | ProductAvailableColors.Black, "assets/garment-pants-olive.jpg", 4.35m, 76),
            new CategoryProductSeed("Accessories", "Everyday Runner Sneakers", 68m, ProductAvailableColors.White | ProductAvailableColors.Black, "assets/garment-sneakers.jpg", 4.58m, 109),
            new CategoryProductSeed("Formal", "Tailored Olive Trousers", 72m, ProductAvailableColors.Green | ProductAvailableColors.Black, "assets/garment-pants-olive.jpg", 4.47m, 47)
        };

        var categories = await context.Categories
            .Where(category => !category.IsDeleted)
            .ToListAsync();
        var coveredCategoryIds = await context.Products
            .Where(product => !product.IsDeleted)
            .Select(product => product.CategoryId)
            .Distinct()
            .ToListAsync();
        var coveredCategories = coveredCategoryIds.ToHashSet();
        var createdCount = 0;

        for (var index = 0; index < catalog.Length; index++)
        {
            var productSeed = catalog[index];
            var category = categories.FirstOrDefault(item =>
                item.Name.Equals(productSeed.CategoryName, StringComparison.OrdinalIgnoreCase));

            if (category is null || coveredCategories.Contains(category.Id))
            {
                continue;
            }

            var productId = SeedId(33, index + 1);
            var product = await context.Products.IgnoreQueryFilters()
                .FirstOrDefaultAsync(item => item.Id == productId);

            if (product is null)
            {
                product = new Product { Id = productId };
                context.Products.Add(product);
                createdCount++;
            }

            product.CategoryId = category.Id;
            product.Name = productSeed.Name;
            product.BasePrice = productSeed.Price;
            product.AvailableColors = productSeed.Colors;
            product.PreviewImageURL = productSeed.PreviewImageUrl;
            product.IsAvailable = true;
            product.StockStatus = "In stock";
            product.AverageRating = productSeed.Rating;
            product.ReviewCount = productSeed.Reviews;
            product.IsDeleted = false;
            product.DeletedAt = null;
            product.CreatedAt = DateTime.UtcNow.AddDays(-(20 - index));

            var productImageId = SeedId(34, index + 1);
            var productImage = await context.ProductImages
                .FirstOrDefaultAsync(image => image.Id == productImageId);

            if (productImage is null)
            {
                productImage = new ProductImage { Id = productImageId };
                context.ProductImages.Add(productImage);
            }

            productImage.ProductId = productId;
            productImage.ImageUrl = productSeed.PreviewImageUrl;
            productImage.Color = productSeed.Colors;
            productImage.ViewAngle = ViewAngle.Front;
            productImage.PrintableZoneJson = "{\"left\":0.2,\"top\":0.16,\"width\":0.6,\"height\":0.62}";
            productImage.IsPrimary = true;
            productImage.DisplayOrder = 0;

            coveredCategories.Add(category.Id);
        }

        await context.SaveChangesAsync();
        logger.LogInformation(
            "Added {Count} demo product(s) to categories that previously had no products.",
            createdCount);
    }

    private static async Task<IReadOnlyList<Guid>> SeedTemplatesAsync(
        AppDbContext context,
        IReadOnlyList<Guid> categoryIds,
        IReadOnlyList<Guid> userIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var templates = new[]
        {
            new TemplateSeed("Midnight Calligraphy", "typography, arabic, midnight", 248, 46, 4.86m, 72, true, 2),
            new TemplateSeed("Sunlit Botanica", "botanical, minimal, nature", 187, 31, 4.72m, 58, true, 5),
            new TemplateSeed("Metro Pulse", "streetwear, urban, geometric", 321, 64, 4.91m, 105, true, 1),
            new TemplateSeed("Riviera Postcard", "travel, vintage, mediterranean", 146, 22, 4.63m, 44, true, 9),
            new TemplateSeed("Desert Frequency", "abstract, music, warm", 98, 17, 4.38m, 29, true, 4),
            new TemplateSeed("Soft Geometry", "minimal, pastel, geometric", 173, 27, 4.79m, 51, true, 3),
            new TemplateSeed("Analog Athletics", "sport, vintage, collegiate", 214, 39, 4.67m, 63, true, 8),
            new TemplateSeed("Bloom After Dark", "floral, dark, illustrative", 132, 19, 4.44m, 37, true, 0),
            new TemplateSeed("Signal / Noise", "tech, glitch, monochrome", 88, 14, 4.12m, 26, true, 6),
            new TemplateSeed("Good Days Club", "positive, type, colorful", 265, 53, 4.88m, 91, true, 7),
            new TemplateSeed("Quiet Coast", "coastal, minimal, blue", 119, 21, 4.55m, 40, true, 3),
            new TemplateSeed("Citrus Assembly", "bold, fruit, pop-art", 74, 12, 4.21m, 22, true, 7),
            new TemplateSeed("Archive Stamp Study", "draft, vintage, stamp", 0, 0, 0m, 0, false, 8),
            new TemplateSeed("Monoline Faces", "draft, line-art, portrait", 0, 0, 0m, 0, false, 0),
            new TemplateSeed("Neo Folk Symbols", "draft, folk, symbols", 0, 0, 0m, 0, false, 4),
            new TemplateSeed("After Hours Type", "draft, typography, nightlife", 0, 0, 0m, 0, false, 1)
        };

        var seeds = templates.Select((template, index) => new Template
        {
            Id = SeedId(4, index + 1),
            CategoryId = categoryIds[template.CategoryIndex],
            CreatorUserId = userIds[index % userIds.Count],
            Name = template.Name,
            StyleTags = template.Tags,
            PreviewImageURL = TemplateImagePaths[index % TemplateImagePaths.Length],
            IsPublic = template.IsPublic,
            LikesCount = template.Likes,
            RemixesCount = template.Remixes,
            AverageRating = template.Rating,
            ReviewCount = template.Reviews,
            CanvasStateJSON = "{\"version\":1,\"objects\":[]}",
            CreatedAt = now.AddDays(index < 12 ? -(1 + index) : -(18 + index))
        }).ToArray();

        await AddMissingAsync(context, context.Templates, seeds, "templates", logger);
        return seeds.Select(seed => seed.Id).ToArray();
    }

    private static async Task SeedCommunityInteractionsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> templateIds,
        IReadOnlyList<Guid> userIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var types = new[]
        {
            InteractionType.Like, InteractionType.Save, InteractionType.Comment, InteractionType.Like,
            InteractionType.Remix, InteractionType.Comment, InteractionType.Like, InteractionType.Save,
            InteractionType.Like, InteractionType.Comment, InteractionType.Remix, InteractionType.Like,
            InteractionType.Save, InteractionType.Comment, InteractionType.Like, InteractionType.Remix,
            InteractionType.Like, InteractionType.Save, InteractionType.Comment, InteractionType.Like
        };
        var comments = new[]
        {
            "The color balance on this is beautiful.",
            "Would absolutely wear this on an oversized tee.",
            "The type treatment feels polished and original.",
            "Saving this for my next studio session.",
            "Love how the details hold up at small sizes."
        };
        var commentIndex = 0;
        var seeds = types.Select((type, index) => new CommunityInteraction
        {
            Id = SeedId(5, index + 1),
            UserId = userIds[(index + 3) % userIds.Count],
            TemplateId = templateIds[index % 12],
            InteractionType = type,
            Content = type == InteractionType.Comment ? comments[commentIndex++ % comments.Length] : null,
            CreatedAt = now.AddHours(-(6 + index * 9))
        }).ToArray();

        await AddMissingAsync(context, context.CommunityInteractions, seeds, "community interactions", logger);
    }

    private static async Task<IReadOnlyList<Guid>> SeedDesignsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> productIds,
        IReadOnlyList<Guid> templateIds,
        IReadOnlyList<Guid> userIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var colors = new[] { "Black", "White", "Navy", "Forest Green", "Burgundy", "Natural" };
        var sizes = new[] { ProductSize.S, ProductSize.M, ProductSize.L, ProductSize.XL };
        var fabrics = new[] { FabricType.Cotton, FabricType.Polyester, FabricType.Linen };
        var methods = new[] { PrintMethodType.DirectToGarment, PrintMethodType.ScreenPrinting, PrintMethodType.Embroidery };
        var seeds = Enumerable.Range(0, 20).Select(index => new Design
        {
            Id = SeedId(6, index + 1),
            UserId = userIds[index % userIds.Count],
            ProductId = productIds[index % productIds.Count],
            TemplateId = templateIds[index % 12],
            CanvasStateJSON = "{\"version\":1,\"objects\":[]}",
            SnapshotImageURL = TemplateImagePaths[(index + 4) % TemplateImagePaths.Length],
            Status = index % 5 == 0 ? DesignStatus.Private : DesignStatus.Public,
            SelectedSize = sizes[index % sizes.Length],
            SelectedFabric = fabrics[index % fabrics.Length],
            SelectedPrintMethod = methods[index % methods.Length],
            SelectedColor = colors[index % colors.Length],
            CalculatedPrice = 32m + (index % 6) * 8.5m,
            CreatedAt = now.AddDays(-(1 + index * 2))
        }).ToArray();

        await AddMissingAsync(context, context.Designs, seeds, "designs", logger);
        return seeds.Select(seed => seed.Id).ToArray();
    }

    private static async Task<IReadOnlyList<Guid>> SeedOrdersAsync(
        AppDbContext context,
        IReadOnlyList<Guid> userIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var statuses = new[]
        {
            OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered,
            OrderStatus.Delivered, OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.Shipped,
            OrderStatus.Pending, OrderStatus.Delivered, OrderStatus.Returned, OrderStatus.Processing,
            OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled, OrderStatus.Pending
        };
        var cities = new[] { "Cairo", "Alexandria", "Giza", "Mansoura", "Cairo", "Tanta" };
        var streets = new[] { "12 Talaat Harb Street", "8 Fouad Street", "21 Nile Corniche", "44 El Gomhoria Street", "19 Road 9, Maadi", "7 El Bahr Street" };
        var seeds = statuses.Select((status, index) =>
        {
            var subtotal = 48m + (index % 7) * 21m;
            var discount = index % 4 == 0 ? 10m : 0m;
            return new Order
            {
                Id = SeedId(7, index + 1),
                UserId = userIds[index % userIds.Count],
                OrderNumber = $"AT-2026-{1101 + index}",
                ReceiverName = DemoUserNames[index % DemoUserNames.Length],
                PhoneNumber = $"+20 10 {2000 + index:0000} {3000 + index:0000}",
                Address = streets[index % streets.Length],
                City = cities[index % cities.Length],
                DeliveryNotes = index % 3 == 0 ? "Please call before delivery." : null,
                SubTotal = subtotal,
                DiscountAmount = discount,
                TotalAmount = subtotal - discount + 7m,
                PointsRedeemed = index % 4 == 0 ? 100 : 0,
                OrderStatus = status,
                PaymentStatus = status switch
                {
                    OrderStatus.Cancelled => PaymentStatus.Failed,
                    OrderStatus.Returned => PaymentStatus.Refunded,
                    OrderStatus.Pending => PaymentStatus.Pending,
                    _ => PaymentStatus.Paid
                },
                CreatedAt = now.AddDays(index < 12 ? -(index + 1) : -(20 + index * 2))
            };
        }).ToArray();

        await AddMissingAsync(context, context.Orders, seeds, "orders", logger);
        return seeds.Select(seed => seed.Id).ToArray();
    }

    private static async Task SeedOrderItemsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> orderIds,
        IReadOnlyList<Guid> designIds,
        Guid? printerProfileId,
        ILogger logger)
    {
        var itemStatuses = new[]
        {
            OrderItemStatus.Pending, OrderItemStatus.AssignedToPrinter, OrderItemStatus.InProduction,
            OrderItemStatus.Ready, OrderItemStatus.Shipped
        };
        var seeds = Enumerable.Range(0, 20).Select(index =>
        {
            var assigned = index % 3 != 0;
            var unitPrice = 32m + (index % 6) * 8.5m;
            return new OrderItem
            {
                Id = SeedId(71, index + 1),
                OrderId = orderIds[index < orderIds.Count ? index : index - orderIds.Count],
                DesignId = designIds[index],
                Quantity = index % 5 == 0 ? 2 : 1,
                UnitPrice = unitPrice,
                PriceBreakdown = $"Base garment and {itemStatuses[index % itemStatuses.Length]} print setup",
                SnapshotImageURL = TemplateImagePaths[(index + 4) % TemplateImagePaths.Length],
                Status = assigned ? itemStatuses[1 + index % 4] : OrderItemStatus.Pending,
                PrinterProfileId = assigned ? printerProfileId : null
            };
        }).ToArray();

        await AddMissingAsync(context, context.OrderItems, seeds, "order items", logger);
    }

    private static async Task SeedModerationReportsAsync(
        AppDbContext context,
        IReadOnlyList<Guid> templateIds,
        IReadOnlyList<Guid> userIds,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var reasons = new[]
        {
            "Possible use of a protected sports mark.",
            "Artwork may contain uncredited source material.",
            "Community member reported misleading attribution.",
            "Requested review for potentially sensitive wording.",
            "Design appears very similar to an existing upload.",
            "Possible promotional spam in the template description.",
            "Reported for low-resolution copied artwork.",
            "Requested review of cultural symbol usage.",
            "Potentially inappropriate text for the public feed.",
            "Duplicate submission reported by another creator."
        };
        var statuses = new[]
        {
            ModerationReportStatus.Pending, ModerationReportStatus.Reviewed,
            ModerationReportStatus.ActionTaken, ModerationReportStatus.Dismissed,
            ModerationReportStatus.Pending, ModerationReportStatus.Reviewed,
            ModerationReportStatus.ActionTaken, ModerationReportStatus.Dismissed,
            ModerationReportStatus.Pending, ModerationReportStatus.Reviewed
        };
        var seeds = statuses.Select((status, index) => new ModerationReport
        {
            Id = SeedId(8, index + 1),
            ReporterUserId = userIds[(index + 2) % userIds.Count],
            TargetTemplateId = templateIds[index % 12],
            Reason = reasons[index],
            Status = status,
            ActionTaken = status switch
            {
                ModerationReportStatus.ActionTaken => "Template visibility restricted pending creator revision.",
                ModerationReportStatus.Dismissed => "Reviewed; no policy violation found.",
                ModerationReportStatus.Reviewed => "Initial review completed; awaiting final decision.",
                _ => null
            },
            ResolvedAt = status is ModerationReportStatus.ActionTaken or ModerationReportStatus.Dismissed
                ? now.AddDays(-(index + 1)).AddHours(4)
                : null,
            CreatedAt = now.AddDays(-(index + 2))
        }).ToArray();

        await AddMissingAsync(context, context.ModerationReports, seeds, "moderation reports", logger);
    }

    private static async Task<int> AddMissingAsync<TEntity>(
        AppDbContext context,
        DbSet<TEntity> set,
        IReadOnlyCollection<TEntity> seeds,
        string entityLabel,
        ILogger logger)
        where TEntity : BaseEntity
    {
        var seedIds = seeds.Select(seed => seed.Id).ToArray();
        var existingIds = await set.IgnoreQueryFilters()
            .Where(entity => seedIds.Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToHashSetAsync();
        var missing = seeds.Where(seed => !existingIds.Contains(seed.Id)).ToArray();

        if (missing.Length > 0)
        {
            set.AddRange(missing);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seeded {Count} demo {EntityLabel}.", missing.Length, entityLabel);
        return missing.Length;
    }

    private static Guid SeedId(int group, int index) =>
        Guid.Parse($"d3{group:00}0000-0000-0000-0000-{index:000000000000}");

    private static readonly string[] DemoUserNames =
    {
        "Maya Okafor", "Theo Wren", "Lin Park", "Sofia Renault", "Omar El-Sayed",
        "Nadia Bell", "Jonas Meyer", "Amara Cole", "Rami Haddad", "Elena Rossi"
    };

    private sealed record DemoUser(
        string Name,
        string Email,
        string Bio,
        string Colors,
        string Interests,
        string DesignPreference);

    private sealed record ProductSeed(
        string Name,
        int CategoryIndex,
        decimal Price,
        ProductAvailableColors Colors,
        decimal Rating,
        int Reviews,
        string StockStatus);

    private sealed record CategoryProductSeed(
        string CategoryName,
        string Name,
        decimal Price,
        ProductAvailableColors Colors,
        string PreviewImageUrl,
        decimal Rating,
        int Reviews);

    private sealed record TemplateSeed(
        string Name,
        string Tags,
        int Likes,
        int Remixes,
        decimal Rating,
        int Reviews,
        bool IsPublic,
        int CategoryIndex);
}

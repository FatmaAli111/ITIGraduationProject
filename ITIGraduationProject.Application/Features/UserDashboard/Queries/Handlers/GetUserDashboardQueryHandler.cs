using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.UserDashboard;
using ITIGraduationProject.Application.Features.UserDashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.UserDashboard.Queries.Handlers
{
    public class GetUserDashboardQueryHandler
        : ResponseHandler,
          IRequestHandler<GetUserDashboardQuery, Response<UserDashboardDto>>
    {
        private readonly IUnitOfWork _uow;

        private static readonly string[] ChecklistLabels =
        {
            "Choose a product base",
            "Place artwork inside print zone",
            "Preview front and back mockups",
            "Add to cart or publish"
        };

        private static readonly IReadOnlyDictionary<ProductAvailableColors, string> ColorHexMap =
            new Dictionary<ProductAvailableColors, string>
            {
                { ProductAvailableColors.Red, "#FF6B4A" },
                { ProductAvailableColors.Blue, "#7AA7D9" },
                { ProductAvailableColors.Green, "#556B2F" },
                { ProductAvailableColors.Yellow, "#F5C542" },
                { ProductAvailableColors.Black, "#1A1A2E" },
                { ProductAvailableColors.White, "#FAF8F5" },
                { ProductAvailableColors.Orange, "#FF6B4A" },
                { ProductAvailableColors.Purple, "#7B68EE" },
                { ProductAvailableColors.Pink, "#FFB6C1" },
                { ProductAvailableColors.Gray, "#808080" },
                { ProductAvailableColors.Brown, "#8B4513" },
                { ProductAvailableColors.Cyan, "#00C9A7" },
                { ProductAvailableColors.Magenta, "#FF00FF" },
                { ProductAvailableColors.Lime, "#32CD32" },
                { ProductAvailableColors.Maroon, "#800000" },
            };

        private static readonly HashSet<OrderStatus> InactiveOrderStatuses = new()
        {
            OrderStatus.Delivered,
            OrderStatus.Cancelled,
            OrderStatus.Returned
        };

        public GetUserDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<UserDashboardDto>> Handle(
            GetUserDashboardQuery request,
            CancellationToken ct)
        {
            var user = await _uow.Users.GetTableNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct);

            if (user is null)
                return NotFound<UserDashboardDto>("User not found.");

            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var designs = await _uow.Designs.GetTableNoTracking()
                .Include(d => d.Product)
                .Include(d => d.Template)
                .Include(d => d.GraphicAssets)
                .Include(d => d.DesignImages)
                .Include(d => d.CartItems)
                .Include(d => d.OrderItems)
                .Where(d => d.UserId == request.UserId && !d.IsDeleted)
                .ToListAsync(ct);

            var orders = await _uow.Orders.GetTableNoTracking()
                .Include(o => o.Shipment)
                .Where(o => o.UserId == request.UserId && !o.IsDeleted)
                .ToListAsync(ct);

            var userTemplates = await _uow.Templates.GetTableNoTracking()
                .Where(t => t.CreatorUserId == request.UserId && !t.IsDeleted)
                .ToListAsync(ct);

            var userTemplateIds = userTemplates.Select(t => t.Id).ToList();

            var communityLikes = userTemplates.Sum(t => t.LikesCount);

            var likesThisWeek = userTemplateIds.Count == 0
                ? 0
                : await _uow.CommunityInteractions.GetTableNoTracking()
                    .CountAsync(
                        i => userTemplateIds.Contains(i.TemplateId)
                             && i.InteractionType == InteractionType.Like
                             && !i.IsDeleted
                             && i.CreatedAt >= weekAgo,
                        ct);

            var savedDesigns = designs.Count;
            var designsThisWeek = designs.Count(d =>
                d.CreatedAt >= weekAgo || (d.UpdatedAt.HasValue && d.UpdatedAt.Value >= weekAgo));

            var activeOrders = orders.Where(o => !InactiveOrderStatuses.Contains(o.OrderStatus)).ToList();
            var pendingReviewCount = activeOrders.Count(o => o.OrderStatus == OrderStatus.Pending);

            var featuredDesign = designs
                .Where(d => d.Status == DesignStatus.Draft)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .FirstOrDefault()
                ?? designs
                    .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                    .FirstOrDefault();

            var checklist = featuredDesign is null
                ? BuildEmptyChecklist()
                : BuildChecklist(featuredDesign);

            var featuredDraft = featuredDesign is null
                ? null
                : MapFeaturedDraft(featuredDesign, checklist);

            var latestActiveOrder = activeOrders
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            var activeOrder = latestActiveOrder is null
                ? null
                : MapActiveOrder(latestActiveOrder);

            var recommendations = await BuildRecommendationsAsync(ct);
            var recentActivity = BuildRecentActivity(designs, orders, userTemplates);

            var dashboard = new UserDashboardDto
            {
                GreetingName = GetFirstName(user.Name),
                Stats = new UserDashboardStatsDto
                {
                    SavedDesigns = savedDesigns,
                    SavedDesignsDelta = designsThisWeek > 0 ? $"+{designsThisWeek} this week" : null,
                    ActiveOrders = activeOrders.Count,
                    ActiveOrdersDelta = pendingReviewCount > 0
                        ? $"{pendingReviewCount} needs review"
                        : null,
                    CommunityLikes = communityLikes,
                    CommunityLikesDelta = likesThisWeek > 0 ? $"+{likesThisWeek} this week" : null
                },
                FeaturedDraft = featuredDraft,
                ActiveOrder = activeOrder,
                Recommendations = recommendations,
                RecentActivity = recentActivity,
                DesignChecklist = checklist
            };

            return Success(dashboard);
        }

        private static string GetFirstName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "there";

            return name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private static UserDashboardFeaturedDraftDto MapFeaturedDraft(
            Design design,
            List<UserDashboardChecklistItemDto> checklist)
        {
            var completedTags = checklist
                .Where(c => c.Complete)
                .Select(c => c.Label)
                .ToList();

            var progress = checklist.Count == 0
                ? 0
                : (int)Math.Round(checklist.Count(c => c.Complete) * 100.0 / checklist.Count);

            return new UserDashboardFeaturedDraftDto
            {
                Id = design.Id,
                Title = ResolveDesignTitle(design),
                Product = design.Product?.Name ?? string.Empty,
                UpdatedAt = design.UpdatedAt ?? design.CreatedAt,
                PreviewImageUrl = design.SnapshotImageURL,
                Progress = progress,
                Tags = completedTags
            };
        }

        private static string ResolveDesignTitle(Design design)
        {
            if (!string.IsNullOrWhiteSpace(design.Template?.Name))
                return design.Template.Name;

            if (!string.IsNullOrWhiteSpace(design.Product?.Name))
                return $"{design.Product.Name} design";

            return "Untitled design";
        }

        private static UserDashboardActiveOrderDto MapActiveOrder(Order order)
        {
            return new UserDashboardActiveOrderDto
            {
                Id = order.Id,
                DisplayCode = order.OrderNumber,
                Status = order.OrderStatus.ToString().ToUpperInvariant(),
                Eta = order.Shipment?.EstimatedDeliveryDate
            };
        }

        private async Task<List<UserDashboardRecommendationDto>> BuildRecommendationsAsync(CancellationToken ct)
        {
            var products = await _uow.Products.GetTableNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsAvailable)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync(ct);

            return products.Select(MapRecommendation).ToList();
        }

        private static UserDashboardRecommendationDto MapRecommendation(Product product)
        {
            return new UserDashboardRecommendationDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category?.Name ?? string.Empty,
                Price = product.BasePrice,
                ImageUrl = product.PreviewImageURL,
                Rating = product.AverageRating,
                Reviews = product.ReviewCount,
                Colors = ExpandColors(product.AvailableColors),
                Badge = ResolveProductBadge(product),
                Reason = "Recommended for your saved styles"
            };
        }

        private static string? ResolveProductBadge(Product product)
        {
            if (product.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                return "NEW";

            if (product.ReviewCount >= 300)
                return "BESTSELLER";

            if (!string.IsNullOrWhiteSpace(product.StockStatus)
                && product.StockStatus.Contains("limited", StringComparison.OrdinalIgnoreCase))
                return "LIMITED";

            return null;
        }

        private static List<string> ExpandColors(ProductAvailableColors colors)
        {
            return Enum.GetValues<ProductAvailableColors>()
                .Where(flag => flag != 0 && colors.HasFlag(flag))
                .Select(flag => ColorHexMap.TryGetValue(flag, out var hex) ? hex : "#CCCCCC")
                .Distinct()
                .Take(3)
                .ToList();
        }

        private static List<UserDashboardActivityItemDto> BuildRecentActivity(
            IReadOnlyList<Design> designs,
            IReadOnlyList<Order> orders,
            IReadOnlyList<Template> templates)
        {
            var items = new List<UserDashboardActivityItemDto>();

            foreach (var design in designs)
            {
                var title = ResolveDesignTitle(design);
                var timestamp = design.UpdatedAt ?? design.CreatedAt;

                items.Add(new UserDashboardActivityItemDto
                {
                    Id = design.Id.ToString(),
                    Type = design.Status == DesignStatus.Public ? "design_published" : "design_saved",
                    Label = design.Status == DesignStatus.Public ? "Published to community" : "Saved design",
                    Target = title,
                    CreatedAt = timestamp
                });
            }

            foreach (var order in orders)
            {
                var (type, label) = order.OrderStatus switch
                {
                    OrderStatus.Shipped => ("order_shipped", "Order shipped"),
                    OrderStatus.Delivered => ("order_delivered", "Order delivered"),
                    OrderStatus.Cancelled => ("order_cancelled", "Order cancelled"),
                    _ => ("order_placed", "Order placed")
                };

                items.Add(new UserDashboardActivityItemDto
                {
                    Id = order.Id.ToString(),
                    Type = type,
                    Label = label,
                    Target = order.OrderNumber,
                    CreatedAt = order.CreatedAt
                });
            }

            foreach (var template in templates.Where(t => t.IsPublic))
            {
                items.Add(new UserDashboardActivityItemDto
                {
                    Id = template.Id.ToString(),
                    Type = "template_published",
                    Label = "Published to community",
                    Target = template.Name,
                    CreatedAt = template.UpdatedAt ?? template.CreatedAt
                });
            }

            return items
                .OrderByDescending(i => i.CreatedAt)
                .Take(4)
                .ToList();
        }

        private static List<UserDashboardChecklistItemDto> BuildEmptyChecklist()
        {
            return ChecklistLabels
                .Select(label => new UserDashboardChecklistItemDto { Label = label, Complete = false })
                .ToList();
        }

        private static List<UserDashboardChecklistItemDto> BuildChecklist(Design design)
        {
            var hasProductBase = design.ProductId != Guid.Empty;
            var hasArtwork = design.GraphicAssets.Any()
                || (!string.IsNullOrWhiteSpace(design.CanvasStateJSON)
                    && design.CanvasStateJSON.Trim() is not ("{}" or "[]"));
            var hasMockups = !string.IsNullOrWhiteSpace(design.SnapshotImageURL)
                || design.DesignImages.Count >= 2;
            var isPublishedOrOrdered = design.Status == DesignStatus.Public
                || design.CartItems.Any()
                || design.OrderItems.Any();

            var completion = new[] { hasProductBase, hasArtwork, hasMockups, isPublishedOrOrdered };

            return ChecklistLabels
                .Select((label, index) => new UserDashboardChecklistItemDto
                {
                    Label = label,
                    Complete = completion[index]
                })
                .ToList();
        }
    }
}

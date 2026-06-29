using System.Text.Json;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.AiReports;
using ITIGraduationProject.Application.Features.Admin.AiReports.Commands.Models;
using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Admin.AiReports.Commands.Handlers;

public sealed class GenerateAiReportCommandHandler
    : ResponseHandler,
      IRequestHandler<GenerateAiReportCommand, Response<AiReportDto>>
{
    private static readonly string[] SupportedReportTypes =
    [
        "Revenue", "Orders", "Creators", "Products", "Templates", "Production", "Community"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly ILangflowService _langflowService;

    public GenerateAiReportCommandHandler(
        IUnitOfWork unitOfWork,
        ISender sender,
        ILangflowService langflowService)
    {
        _unitOfWork = unitOfWork;
        _sender = sender;
        _langflowService = langflowService;
    }

    public async Task<Response<AiReportDto>> Handle(
        GenerateAiReportCommand request,
        CancellationToken cancellationToken)
    {
        var reportType = SupportedReportTypes.FirstOrDefault(type =>
            type.Equals(request.ReportType?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (reportType is null)
        {
            return BadRequest<AiReportDto>(
                $"Unsupported report type. Choose one of: {string.Join(", ", SupportedReportTypes)}.");
        }

        if (request.Filters?.FromDate is not null &&
            request.Filters.ToDate is not null &&
            request.Filters.FromDate.Value.Date > request.Filters.ToDate.Value.Date)
        {
            return BadRequest<AiReportDto>("The report start date must be on or before the end date.");
        }

        var metrics = await BuildMetricsAsync(reportType, request.Filters, cancellationToken);
        var prompt = BuildNarrationPrompt(reportType, request.Filters, metrics);
        var rawNarration = await _langflowService.SendMessageAsync(prompt, Guid.NewGuid().ToString());

        if (!TryParseNarrative(rawNarration, out var narrative))
        {
            return BadRequest<AiReportDto>(
                "The AI report narration was not valid structured JSON. No unstructured AI text was returned.");
        }

        return Success(new AiReportDto
        {
            ReportType = reportType,
            GeneratedAt = DateTime.UtcNow,
            Filters = request.Filters,
            Metrics = metrics,
            Summary = narrative!.Summary.Trim(),
            Highlights = narrative.Highlights
                .Where(highlight => !string.IsNullOrWhiteSpace(highlight))
                .Select(highlight => highlight.Trim())
                .ToList(),
            Recommendation = string.IsNullOrWhiteSpace(narrative.Recommendation)
                ? null
                : narrative.Recommendation.Trim()
        });
    }

    private Task<AiReportMetricsDto> BuildMetricsAsync(
        string reportType,
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken) => reportType switch
        {
            "Orders" => BuildOrdersMetricsAsync(filters, cancellationToken),
            "Revenue" => BuildRevenueMetricsAsync(filters, cancellationToken),
            "Creators" => BuildCreatorsMetricsAsync(filters, cancellationToken),
            "Products" => BuildProductsMetricsAsync(filters, cancellationToken),
            "Templates" => BuildTemplatesMetricsAsync(filters, cancellationToken),
            "Production" => BuildProductionMetricsAsync(filters, cancellationToken),
            "Community" => BuildCommunityMetricsAsync(filters, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported report type '{reportType}'.")
        };

    private async Task<AiReportMetricsDto> BuildOrdersMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var orders = ApplyDateFilter(
            _unitOfWork.Orders.GetTableNoTracking().Where(order => !order.IsDeleted),
            filters);

        List<NamedCountDto> statusCounts;
        if (!HasDateFilter(filters))
        {
            var existingResult = await _sender.Send(new GetOrdersByStatusQuery(), cancellationToken);
            statusCounts = existingResult.Data
                .Select(item => new NamedCountDto { Name = item.Status, Count = item.Count })
                .ToList();
        }
        else
        {
            statusCounts = await BuildEnumCountsAsync(
                orders.Select(order => order.OrderStatus),
                Enum.GetValues<OrderStatus>(),
                cancellationToken);
        }

        var totalOrders = await orders.CountAsync(cancellationToken);
        return new OrdersMetricsDto
        {
            TotalOrders = totalOrders,
            TotalOrderValue = await orders.SumAsync(order => order.TotalAmount, cancellationToken),
            AverageOrderValue = totalOrders == 0
                ? 0
                : await orders.AverageAsync(order => order.TotalAmount, cancellationToken),
            TotalItems = await orders
                .SelectMany(order => order.OrderItems)
                .SumAsync(item => item.Quantity, cancellationToken),
            OrdersByStatus = statusCounts
        };
    }

    private async Task<AiReportMetricsDto> BuildRevenueMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var orders = ApplyDateFilter(
                _unitOfWork.Orders.GetTableNoTracking().Where(order => !order.IsDeleted),
                filters)
            .Where(order => order.OrderStatus != OrderStatus.Cancelled &&
                            order.OrderStatus != OrderStatus.Returned);

        var orderCount = await orders.CountAsync(cancellationToken);
        decimal totalRevenue;
        if (!HasDateFilter(filters))
        {
            var overview = await _sender.Send(new GetDashboardOverviewQuery(), cancellationToken);
            totalRevenue = overview.Data.TotalRevenue;
        }
        else
        {
            totalRevenue = await orders.SumAsync(order => order.TotalAmount, cancellationToken);
        }

        return new RevenueMetricsDto
        {
            TotalRevenue = totalRevenue,
            GrossSubtotal = await orders.SumAsync(order => order.SubTotal, cancellationToken),
            TotalDiscounts = await orders.SumAsync(order => order.DiscountAmount, cancellationToken),
            AverageOrderValue = orderCount == 0
                ? 0
                : await orders.AverageAsync(order => order.TotalAmount, cancellationToken),
            RevenueGeneratingOrders = orderCount
        };
    }

    private async Task<AiReportMetricsDto> BuildCreatorsMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var templates = ApplyDateFilter(
            _unitOfWork.Templates.GetTableNoTracking()
                .Where(template => !template.IsDeleted && template.IsPublic),
            filters);

        List<TopCreatorMetricDto> topCreators;
        if (!HasDateFilter(filters))
        {
            var existingResult = await _sender.Send(new GetTopCreatorsQuery(5), cancellationToken);
            topCreators = existingResult.Data.Select(creator => new TopCreatorMetricDto
            {
                UserId = creator.UserId,
                UserName = creator.UserName,
                TemplateCount = creator.TemplateCount,
                TotalLikes = creator.TotalLikes,
                TotalRemixes = creator.TotalRemixes
            }).ToList();
        }
        else
        {
            topCreators = await templates
                .GroupBy(template => template.CreatorUserId)
                .Select(group => new TopCreatorMetricDto
                {
                    UserId = group.Key,
                    UserName = group.First().CreatorUser.Name,
                    TemplateCount = group.Count(),
                    TotalLikes = group.Sum(template => template.LikesCount),
                    TotalRemixes = group.Sum(template => template.RemixesCount)
                })
                .OrderByDescending(creator => creator.TotalLikes + creator.TotalRemixes * 2)
                .Take(5)
                .ToListAsync(cancellationToken);
        }

        return new CreatorsMetricsDto
        {
            ActiveCreators = await templates.Select(template => template.CreatorUserId).Distinct().CountAsync(cancellationToken),
            PublicTemplates = await templates.CountAsync(cancellationToken),
            TotalLikes = await templates.SumAsync(template => template.LikesCount, cancellationToken),
            TotalRemixes = await templates.SumAsync(template => template.RemixesCount, cancellationToken),
            TopCreators = topCreators
        };
    }

    private async Task<AiReportMetricsDto> BuildProductsMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var products = ApplyDateFilter(
            _unitOfWork.Products.GetTableNoTracking().Where(product => !product.IsDeleted),
            filters);
        var totalProducts = await products.CountAsync(cancellationToken);

        return new ProductsMetricsDto
        {
            TotalProducts = totalProducts,
            AvailableProducts = await products.CountAsync(product => product.IsAvailable, cancellationToken),
            UnavailableProducts = await products.CountAsync(product => !product.IsAvailable, cancellationToken),
            AverageBasePrice = totalProducts == 0
                ? 0
                : await products.AverageAsync(product => product.BasePrice, cancellationToken),
            AverageRating = totalProducts == 0
                ? 0
                : await products.AverageAsync(product => product.AverageRating, cancellationToken)
        };
    }

    private async Task<AiReportMetricsDto> BuildTemplatesMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var templates = ApplyDateFilter(
            _unitOfWork.Templates.GetTableNoTracking().Where(template => !template.IsDeleted),
            filters);
        var totalTemplates = await templates.CountAsync(cancellationToken);
        var publicTemplates = await templates.CountAsync(template => template.IsPublic, cancellationToken);

        return new TemplatesMetricsDto
        {
            TotalTemplates = totalTemplates,
            PublicTemplates = publicTemplates,
            PrivateTemplates = totalTemplates - publicTemplates,
            TotalLikes = await templates.SumAsync(template => template.LikesCount, cancellationToken),
            TotalRemixes = await templates.SumAsync(template => template.RemixesCount, cancellationToken)
        };
    }

    private async Task<AiReportMetricsDto> BuildProductionMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var orderItems = _unitOfWork.OrderItems.GetTableNoTracking()
            .Where(item => !item.Order.IsDeleted);

        if (filters?.FromDate is not null)
        {
            var fromDate = filters.FromDate.Value.Date;
            orderItems = orderItems.Where(item => item.Order.CreatedAt >= fromDate);
        }

        if (filters?.ToDate is not null)
        {
            var toDateExclusive = filters.ToDate.Value.Date.AddDays(1);
            orderItems = orderItems.Where(item => item.Order.CreatedAt < toDateExclusive);
        }

        var itemCount = await orderItems.CountAsync(cancellationToken);
        var assignedItems = await orderItems.CountAsync(item => item.PrinterProfileId != null, cancellationToken);

        return new ProductionMetricsDto
        {
            TotalOrderItems = itemCount,
            TotalUnits = await orderItems.SumAsync(item => item.Quantity, cancellationToken),
            AssignedItems = assignedItems,
            UnassignedItems = itemCount - assignedItems,
            ActivePrinters = await _unitOfWork.PrinterProfiles.GetTableNoTracking()
                .CountAsync(printer => !printer.IsDeleted && printer.IsActive, cancellationToken),
            ItemsByStatus = await BuildEnumCountsAsync(
                orderItems.Select(item => item.Status),
                Enum.GetValues<OrderItemStatus>(),
                cancellationToken)
        };
    }

    private async Task<AiReportMetricsDto> BuildCommunityMetricsAsync(
        AiReportFiltersDto? filters,
        CancellationToken cancellationToken)
    {
        var interactions = ApplyDateFilter(
            _unitOfWork.CommunityInteractions.GetTableNoTracking()
                .Where(interaction => !interaction.IsDeleted),
            filters);

        var counts = await interactions
            .GroupBy(interaction => interaction.InteractionType)
            .Select(group => new { Type = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Type, item => item.Count, cancellationToken);

        return new CommunityMetricsDto
        {
            TotalInteractions = counts.Values.Sum(),
            Likes = counts.GetValueOrDefault(InteractionType.Like),
            Saves = counts.GetValueOrDefault(InteractionType.Save),
            Remixes = counts.GetValueOrDefault(InteractionType.Remix),
            Comments = counts.GetValueOrDefault(InteractionType.Comment)
        };
    }

    private static async Task<List<NamedCountDto>> BuildEnumCountsAsync<TEnum>(
        IQueryable<TEnum> values,
        IReadOnlyCollection<TEnum> allValues,
        CancellationToken cancellationToken)
        where TEnum : struct, Enum
    {
        var counts = await values
            .GroupBy(value => value)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var lookup = counts.ToDictionary(item => item.Value, item => item.Count);

        return allValues.Select(value => new NamedCountDto
        {
            Name = value.ToString(),
            Count = lookup.GetValueOrDefault(value)
        }).ToList();
    }

    private static IQueryable<T> ApplyDateFilter<T>(
        IQueryable<T> query,
        AiReportFiltersDto? filters)
        where T : BaseAuditableEntity
    {
        if (filters?.FromDate is not null)
        {
            var fromDate = filters.FromDate.Value.Date;
            query = query.Where(entity => entity.CreatedAt >= fromDate);
        }

        if (filters?.ToDate is not null)
        {
            var toDateExclusive = filters.ToDate.Value.Date.AddDays(1);
            query = query.Where(entity => entity.CreatedAt < toDateExclusive);
        }

        return query;
    }

    private static bool HasDateFilter(AiReportFiltersDto? filters) =>
        filters?.FromDate is not null || filters?.ToDate is not null;

    private static string BuildNarrationPrompt(
        string reportType,
        AiReportFiltersDto? filters,
        AiReportMetricsDto metrics)
    {
        var metricsJson = JsonSerializer.Serialize(metrics, JsonOptions);
        var filtersJson = JsonSerializer.Serialize(filters, JsonOptions);

        return $$"""
            You are narrating an admin {{reportType}} report. The backend has already queried and verified every metric.
            Use only the supplied values. Do not call tools, retrieve other data, calculate new values, estimate, or invent numbers.
            Return JSON only, with no markdown fences and no text before or after it, using exactly this schema:
            {
              "summary": "one concise paragraph",
              "highlights": ["concise highlight", "concise highlight"],
              "recommendation": "one practical recommendation or null"
            }
            Keep highlights factual. If the data is insufficient for a recommendation, return null.
            Filters: {{filtersJson}}
            Verified metrics: {{metricsJson}}
            """;
    }

    private static bool TryParseNarrative(string rawText, out AiReportNarrativeDto? narrative)
    {
        narrative = null;
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return false;
        }

        var candidate = StripMarkdownFence(rawText.Trim());
        if (TryDeserialize(candidate, out narrative))
        {
            return true;
        }

        var firstBrace = candidate.IndexOf('{');
        var lastBrace = candidate.LastIndexOf('}');
        return firstBrace >= 0 && lastBrace > firstBrace &&
               TryDeserialize(candidate[firstBrace..(lastBrace + 1)], out narrative);
    }

    private static string StripMarkdownFence(string value)
    {
        if (!value.StartsWith("```", StringComparison.Ordinal))
        {
            return value;
        }

        var firstLineBreak = value.IndexOf('\n');
        if (firstLineBreak < 0)
        {
            return value;
        }

        var withoutOpeningFence = value[(firstLineBreak + 1)..].Trim();
        return withoutOpeningFence.EndsWith("```", StringComparison.Ordinal)
            ? withoutOpeningFence[..^3].Trim()
            : withoutOpeningFence;
    }

    private static bool TryDeserialize(string json, out AiReportNarrativeDto? narrative)
    {
        narrative = null;
        try
        {
            narrative = JsonSerializer.Deserialize<AiReportNarrativeDto>(json, JsonOptions);
            return narrative is not null &&
                   !string.IsNullOrWhiteSpace(narrative.Summary) &&
                   narrative.Highlights is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class AiReportNarrativeDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Highlights { get; set; } = [];
        public string? Recommendation { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace ITIGraduationProject.Application.DTOS.Admin.AiReports;

public sealed class AiReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public AiReportFiltersDto? Filters { get; set; }
    public AiReportMetricsDto Metrics { get; set; } = null!;
    public string Summary { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = [];
    public string? Recommendation { get; set; }
}

public sealed class AiReportFiltersDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "metricType")]
[JsonDerivedType(typeof(OrdersMetricsDto), "orders")]
[JsonDerivedType(typeof(RevenueMetricsDto), "revenue")]
[JsonDerivedType(typeof(CreatorsMetricsDto), "creators")]
[JsonDerivedType(typeof(ProductsMetricsDto), "products")]
[JsonDerivedType(typeof(TemplatesMetricsDto), "templates")]
[JsonDerivedType(typeof(ProductionMetricsDto), "production")]
[JsonDerivedType(typeof(CommunityMetricsDto), "community")]
public abstract class AiReportMetricsDto;

public sealed class OrdersMetricsDto : AiReportMetricsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalItems { get; set; }
    public List<NamedCountDto> OrdersByStatus { get; set; } = [];
}

public sealed class RevenueMetricsDto : AiReportMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal GrossSubtotal { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int RevenueGeneratingOrders { get; set; }
}

public sealed class CreatorsMetricsDto : AiReportMetricsDto
{
    public int ActiveCreators { get; set; }
    public int PublicTemplates { get; set; }
    public int TotalLikes { get; set; }
    public int TotalRemixes { get; set; }
    public List<TopCreatorMetricDto> TopCreators { get; set; } = [];
}

public sealed class ProductsMetricsDto : AiReportMetricsDto
{
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
    public int UnavailableProducts { get; set; }
    public decimal AverageBasePrice { get; set; }
    public decimal AverageRating { get; set; }
}

public sealed class TemplatesMetricsDto : AiReportMetricsDto
{
    public int TotalTemplates { get; set; }
    public int PublicTemplates { get; set; }
    public int PrivateTemplates { get; set; }
    public int TotalLikes { get; set; }
    public int TotalRemixes { get; set; }
}

public sealed class ProductionMetricsDto : AiReportMetricsDto
{
    public int TotalOrderItems { get; set; }
    public int TotalUnits { get; set; }
    public int AssignedItems { get; set; }
    public int UnassignedItems { get; set; }
    public int ActivePrinters { get; set; }
    public List<NamedCountDto> ItemsByStatus { get; set; } = [];
}

public sealed class CommunityMetricsDto : AiReportMetricsDto
{
    public int TotalInteractions { get; set; }
    public int Likes { get; set; }
    public int Saves { get; set; }
    public int Remixes { get; set; }
    public int Comments { get; set; }
}

public sealed class NamedCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class TopCreatorMetricDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TemplateCount { get; set; }
    public int TotalLikes { get; set; }
    public int TotalRemixes { get; set; }
}

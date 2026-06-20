using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOs.Design;

public class DesignDetailDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? TemplateId { get; set; }
    public string CanvasStateJSON { get; set; } = string.Empty;
    public string SnapshotImageURL { get; set; } = string.Empty;
    public DesignStatus Status { get; set; }
    public ProductSize? SelectedSize { get; set; }
    public FabricType? SelectedFabric { get; set; }
    public PrintMethodType? SelectedPrintMethod { get; set; }
    public string? SelectedColor { get; set; }
    public decimal CalculatedPrice { get; set; }
    public ProductDTO Product { get; set; } = null!;
    public TemplateDTO? Template { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string PreviewImageURL { get; set; } = string.Empty;
}

public class TemplateDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PreviewImageURL { get; set; } = string.Empty;
}

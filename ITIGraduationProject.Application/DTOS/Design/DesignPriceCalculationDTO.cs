using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOs.Design;

public class DesignPriceCalculationDTO
{
    public Guid ProductId { get; set; }
    public ProductSize? SelectedSize { get; set; }
    public FabricType? SelectedFabric { get; set; }
    public PrintMethodType? SelectedPrintMethod { get; set; }
}

public class PriceCalculationResultDTO
{
    public decimal BasePrice { get; set; }
    public decimal FabricSurcharge { get; set; }
    public decimal PrintMethodSurcharge { get; set; }
    public decimal SizeSurcharge { get; set; }
    public decimal TotalPrice { get; set; }
}

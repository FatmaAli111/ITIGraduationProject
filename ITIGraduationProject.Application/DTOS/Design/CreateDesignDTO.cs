using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOs.Design;

public class CreateDesignDTO
{
    public Guid ProductId { get; set; }
    public Guid? TemplateId { get; set; }
    public ProductSize? SelectedSize { get; set; }
    public FabricType? SelectedFabric { get; set; }
    public PrintMethodType? SelectedPrintMethod { get; set; }
    public string? SelectedColor { get; set; }
}

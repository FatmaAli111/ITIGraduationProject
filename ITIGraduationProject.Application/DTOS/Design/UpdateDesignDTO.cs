using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOs.Design;

public class UpdateDesignDTO
{
    public string? CanvasStateJSON { get; set; }
    public string? SnapshotImageURL { get; set; }
    public ProductSize? SelectedSize { get; set; }
    public FabricType? SelectedFabric { get; set; }
    public PrintMethodType? SelectedPrintMethod { get; set; }
    public string? SelectedColor { get; set; }
}

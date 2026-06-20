using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOs.Design;

public class DesignListDTO
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SnapshotImageURL { get; set; } = string.Empty;
    public DesignStatus Status { get; set; }
    public decimal CalculatedPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

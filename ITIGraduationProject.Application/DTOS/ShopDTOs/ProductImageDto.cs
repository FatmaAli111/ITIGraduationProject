using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.DTOS.ShopDTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public ViewAngle? ViewAngle { get; set; }
        public string? PrintableZoneJson { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}

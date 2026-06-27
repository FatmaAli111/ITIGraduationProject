using System;

namespace ITIGraduationProject.Application.DTOS.ShopDTOs
{
    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Guid? DesignId { get; set; }
        public string? DesignSnapshotImageUrl { get; set; }
    }
}

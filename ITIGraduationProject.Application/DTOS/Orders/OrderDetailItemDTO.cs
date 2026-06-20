using System;

namespace ITIGraduationProject.Application.DTOS.Orders
{
    public class OrderDetailItemDTO
    {
        public Guid DesignId { get; set; }
        public string DesignName { get; set; } = string.Empty; 
        public string VariationDetails { get; set; } = string.Empty; // ex: Size L - Charcoal
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string SnapshotImageURL { get; set; } = string.Empty; // Image of the design
    }
}

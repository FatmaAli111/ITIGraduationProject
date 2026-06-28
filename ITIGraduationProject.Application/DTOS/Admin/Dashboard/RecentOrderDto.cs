using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.DTOS.Admin.Dashboard
{
    public class RecentOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<RecentOrderItemDto> OrderItems { get; set; } = new();
    }

    public class RecentOrderItemDto
    {
        public Guid Id { get; set; }
        public string DesignName { get; set; } = string.Empty;
        public string SnapshotImageURL { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? PrinterProfileId { get; set; }
        public string? PrinterName { get; set; }
    }
}

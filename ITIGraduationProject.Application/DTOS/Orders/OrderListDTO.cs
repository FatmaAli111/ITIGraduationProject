using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.DTOS.Orders
{
    public class OrderListDTO
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // WLY-2026-00482
        public DateTime PlacedDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty; // Pending, Processing, Shipped

        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; } = 0.0m; 
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public string? TrackingNumber { get; set; }

        public List<OrderDetailItemDTO> OrderItems { get; set; } = new();
    }
}

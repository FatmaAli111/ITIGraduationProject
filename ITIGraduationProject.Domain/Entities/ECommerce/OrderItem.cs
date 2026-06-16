#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid DesignId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string PriceBreakdown { get; set; } = string.Empty;
    public string SnapshotImageURL { get; set; } = string.Empty;
    public OrderItemStatus Status { get; set; }
    public Guid? PrinterProfileId { get; set; }
    public PrinterProfile PrinterProfile { get; set; } = null!;

    public Order Order { get; set; } = null!;
    public Design Design { get; set; } = null!;
}

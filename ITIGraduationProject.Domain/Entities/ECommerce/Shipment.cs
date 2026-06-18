#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class Shipment : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Provider { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public Order Order { get; set; } = null!;
    public ICollection<ShipmentLog> ShipmentLogs { get; set; } = new HashSet<ShipmentLog>();
}

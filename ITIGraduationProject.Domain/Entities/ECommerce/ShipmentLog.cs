#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class ShipmentLog : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public ShipmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Shipment Shipment { get; set; } = null!;
}

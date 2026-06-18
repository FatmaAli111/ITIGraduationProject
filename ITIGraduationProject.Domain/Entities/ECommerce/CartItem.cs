#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid DesignId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SnapshotImageUrl { get; set; } = string.Empty;
    public Cart Cart { get; set; } = null!;
    public Design Design { get; set; } = null!;
}

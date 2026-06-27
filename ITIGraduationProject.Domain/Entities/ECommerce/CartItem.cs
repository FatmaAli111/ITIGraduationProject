#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? DesignId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SnapshotImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Design? Design { get; set; }
}


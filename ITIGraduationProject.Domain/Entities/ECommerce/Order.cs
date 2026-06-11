#nullable enable
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class Order : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? RewardId { get; set; }
    public Guid? CouponId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int PointsRedeemed { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public Reward? Reward { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    public Shipment? Shipment { get; set; }
}

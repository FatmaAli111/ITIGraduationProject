#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class Coupon : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Domain.Entities.ECommerce;

public class Cart : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
}

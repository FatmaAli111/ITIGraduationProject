#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class Reward : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid RewardRuleId { get; set; }
    public RewardType RewardType { get; set; }
    public decimal RewardValue { get; set; }
    public bool IsClaimed { get; set; }
    public string? BadgeImageUrl { get; set; }
    public User User { get; set; } = null!;
    public Template? Template { get; set; }
    public RewardRule RewardRule { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

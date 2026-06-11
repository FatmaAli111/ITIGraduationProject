#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class RewardRule : BaseAuditableEntity
{
    public int LikesThreshold { get; set; }
    public int SavesThreshold { get; set; }
    public RewardType RewardType { get; set; }
    public decimal RewardValue { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Reward> Rewards { get; set; } = new HashSet<Reward>();
}

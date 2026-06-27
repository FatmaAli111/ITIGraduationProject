using System;

namespace ITIGraduationProject.Application.DTOS.Rewards
{
    public class RewardDto
    {
        public Guid Id { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public decimal RewardValue { get; set; }
        public bool IsClaimed { get; set; }
        public string? BadgeImageUrl { get; set; }
        public DateTime EarnedAt { get; set; }
        public string? RuleName { get; set; }
        public string? RuleDescription { get; set; }
        public string? Code { get; set; }
    }
}

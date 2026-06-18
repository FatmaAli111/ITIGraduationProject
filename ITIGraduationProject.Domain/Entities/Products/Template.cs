
#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Domain.Entities.Products;

public class Template : BaseAuditableEntity
{
    public Guid CategoryId { get; set; }
    public Guid CreatorUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? StyleTags { get; set; }
    public string PreviewImageURL { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int LikesCount { get; set; }
    public int RemixesCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public Category Category { get; set; } = null!;
    public User CreatorUser { get; set; } = null!;
    public ICollection<Design> Designs { get; set; } = new HashSet<Design>();
    public ICollection<Reward> Rewards { get; set; } = new HashSet<Reward>();
    public ICollection<ModerationReport> ModerationReports { get; set; } = new HashSet<ModerationReport>();
    public ICollection<CommunityInteraction> CommunityInteractions { get; set; } = new HashSet<CommunityInteraction>();
}

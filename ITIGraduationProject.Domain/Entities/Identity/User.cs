#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Domain.Entities.Identity;

public class User : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public bool IsActive { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int CurrentPointsBalance { get; set; }
    public UserPreferences UserPreferences { get; set; } = null!;
    public PrinterProfile? PrinterProfile { get; set; }
    public Cart Cart { get; set; } = null!;

    public ICollection<Design> Designs { get; set; } = new HashSet<Design>();
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    public ICollection<Template> Templates { get; set; } = new HashSet<Template>();
    public ICollection<AiChatSession> AiChatSessions { get; set; } = new HashSet<AiChatSession>();
    public ICollection<CommunityInteraction> CommunityInteractions { get; set; } = new HashSet<CommunityInteraction>();
    public ICollection<Reward> Rewards { get; set; } = new HashSet<Reward>();
    public ICollection<ModerationReport> ModerationReports { get; set; } = new HashSet<ModerationReport>();
}

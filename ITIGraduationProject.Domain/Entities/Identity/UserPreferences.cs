#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Domain.Entities.Identity;

public class UserPreferences : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    public string? FavoriteColors { get; set; }
    public string? BannedColors { get; set; }
    public string? StyleType { get; set; }
    public string? Interests { get; set; }
    public string? DesignPreference { get; set; }
    public string? ContentPreference { get; set; }
    public int StyleMatchPercentage { get; set; }
    public string? IssuedBadges { get; set; }

    public User User { get; set; } = null!;
}

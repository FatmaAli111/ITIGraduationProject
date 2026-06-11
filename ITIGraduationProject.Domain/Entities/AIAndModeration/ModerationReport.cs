#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class ModerationReport : BaseAuditableEntity
{
    public Guid ReporterUserId { get; set; }
    public Guid TargetTemplateId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ModerationReportStatus Status { get; set; }
    public string? ActionTaken { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public User ReporterUser { get; set; } = null!;
    public Template TargetTemplate { get; set; } = null!;
}

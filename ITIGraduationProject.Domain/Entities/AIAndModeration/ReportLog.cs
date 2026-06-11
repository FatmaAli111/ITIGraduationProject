#nullable enable
using System;
using ITIGraduationProject.Domain.Common;

namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class ReportLog : BaseAuditableEntity
{
    public Guid AdminId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string? Filters { get; set; }
    public string FileURL { get; set; } = string.Empty;
}

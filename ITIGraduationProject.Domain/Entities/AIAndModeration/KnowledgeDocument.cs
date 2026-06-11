#nullable enable
using System;
using ITIGraduationProject.Domain.Common;

namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class KnowledgeDocument : BaseAuditableEntity
{
    public Guid CreatedByAdminId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string SourceText { get; set; } = string.Empty;
    public bool IsEmbedded { get; set; } = false;
    public DateTime? LastEmbeddedAt { get; set; }
    public string? Tags { get; set; }
}

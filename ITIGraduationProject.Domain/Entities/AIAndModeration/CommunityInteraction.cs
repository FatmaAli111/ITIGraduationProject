#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class CommunityInteraction : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public InteractionType InteractionType { get; set; }
    public string? Content { get; set; }
    public User User { get; set; } = null!;
    public Template Template { get; set; } = null!;
}

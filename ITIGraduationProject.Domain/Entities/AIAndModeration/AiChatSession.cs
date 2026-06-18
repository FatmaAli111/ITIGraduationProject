#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class AiChatSession : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? CurrentDesignId { get; set; }
    public AiChatSessionType SessionType { get; set; }
    public User User { get; set; } = null!;
    public Design? CurrentDesign { get; set; }
    public ICollection<AiChatMessage> AiChatMessages { get; set; } = new HashSet<AiChatMessage>();
}

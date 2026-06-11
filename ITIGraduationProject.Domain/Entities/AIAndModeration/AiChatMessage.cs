#nullable enable
using System;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Domain.Entities.AIAndModeration;

public class AiChatMessage : BaseEntity
{
    public Guid AiChatSessionId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public AiChatSession AiChatSession { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.DTOS.AiChat
{
    public class ChatHistoryDto
    {
        public Guid SessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = string.Empty; // "User" or "AI_Assistant"
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}

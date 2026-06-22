using System;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages
{
    public class AiChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}

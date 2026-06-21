using System;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions
{
    public class AiChatSessionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? CurrentDesignId { get; set; }
        public int SessionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

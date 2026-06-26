using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.AiChat
{
    public class ChatHistoryDto
    {
        public Guid SessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

        public List<ChatMessageDto> Messages { get; set; } = [];
    }
}

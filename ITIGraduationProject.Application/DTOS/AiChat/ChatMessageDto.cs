using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.AiChat
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }

        public string Sender { get; set; }

        public string MessageText { get; set; }

        public DateTime SentAt { get; set; }
    }
}
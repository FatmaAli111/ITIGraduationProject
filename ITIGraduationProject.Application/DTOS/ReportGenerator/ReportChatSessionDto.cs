using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.ReportGenerator
{
    public class ReportChatMessageDto
    {
        public Guid Id { get; set; }

        public string Sender { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; }
    }
}

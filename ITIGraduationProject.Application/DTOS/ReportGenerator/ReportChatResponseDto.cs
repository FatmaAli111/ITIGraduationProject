using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.ReportGenerator
{

        public class ReportChatResponseDto
        {
            public Guid SessionId { get; set; }

            public Guid UserMessageId { get; set; }

            public Guid AiMessageId { get; set; }

            public string Response { get; set; } = string.Empty;

            public DateTime ResponseTime { get; set; }
        }

}

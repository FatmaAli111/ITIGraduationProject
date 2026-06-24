using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.ReportGenerator
{
    public class ReportChatSessionDto
    {
        public Guid SessionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int MessagesCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Service.Identity.Email
{
    public class EmailSettings
    {
        public string EmailSender { get; set; }
        public string NameSender { get; set; }
        public string AppPassword { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }
}

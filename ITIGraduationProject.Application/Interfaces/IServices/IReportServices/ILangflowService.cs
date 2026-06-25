using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.IReportServices
{
    namespace ITIGraduationProject.Application.Interfaces.IServices
    {
        public interface ILangflowService
        {
            Task<string> SendMessageAsync(
                string message,
                string sessionId);
        }
    }
}

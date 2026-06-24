using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ReportGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.IReportServices
{
   
        public interface IReportChatService
        {
            Task<Response<Guid>> CreateSessionAsync(Guid userId);

            Task<Response<ReportChatResponseDto>> SendMessageAsync(
                Guid sessionId,
                string message);

            Task<Response<List<ReportChatMessageDto>>> GetHistoryAsync(
                Guid sessionId);
        }
    
}

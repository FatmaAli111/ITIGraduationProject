using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ReportGenerator;
using ITIGraduationProject.Application.Features.ReportGeneratorChat.Queries.Models;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.ReportGeneratorChat.Queries.Handlers
{
   
        public class ReportChatQueryHandler :
            IRequestHandler<
                GetReportChatHistoryQuery,
                Response<List<ReportChatMessageDto>>>
        {
            private readonly IReportChatService _service;

            public ReportChatQueryHandler(
                IReportChatService service)
            {
                _service = service;
            }

            public async Task<Response<List<ReportChatMessageDto>>> Handle(
                GetReportChatHistoryQuery request,
                CancellationToken cancellationToken)
            {
                return await _service.GetHistoryAsync(
                    request.SessionId);
            }
        }
    
}

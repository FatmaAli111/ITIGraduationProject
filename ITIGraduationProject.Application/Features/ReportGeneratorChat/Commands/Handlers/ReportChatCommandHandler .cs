using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ReportGenerator;
using ITIGraduationProject.Application.Features.ReportGeneratorChat.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.ReportGeneratorChat.Commands.Handlers
{
   
        public class ReportChatCommandHandler :
            IRequestHandler<CreateReportChatSessionCommand, Response<Guid>>,
            IRequestHandler<SendReportChatMessageCommand, Response<ReportChatResponseDto>>
        {
            private readonly IReportChatService _service;

            public ReportChatCommandHandler(
                IReportChatService service)
            {
                _service = service;
            }

            public async Task<Response<Guid>> Handle(
                CreateReportChatSessionCommand request,
                CancellationToken cancellationToken)
            {
                return await _service.CreateSessionAsync();
            }

            public async Task<Response<ReportChatResponseDto>> Handle(
                SendReportChatMessageCommand request,
                CancellationToken cancellationToken)
            {
                return await _service.SendMessageAsync(
                    request.SessionId,
                    request.Message);
            }
        }
    }

using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ReportGenerator;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.ReportGeneratorChat.Commands.Models
{
    public record SendReportChatMessageCommand(
     Guid SessionId,
     string Message
 ) : IRequest<Response<ReportChatResponseDto>>;
}

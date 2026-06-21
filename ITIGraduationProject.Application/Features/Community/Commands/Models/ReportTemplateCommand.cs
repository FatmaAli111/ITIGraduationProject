using ITIGraduationProject.Application.Bases;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record ReportTemplateCommand(Guid TemplateId, string Reason) : IRequest<Response<string>>;
}

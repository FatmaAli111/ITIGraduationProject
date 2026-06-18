using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Wrapers.Templates.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Templates.CQRS
{
    public record GetTemplateByIdQuery(Guid Id)
    : IRequest<Response<TemplateDetailDto>>;
}

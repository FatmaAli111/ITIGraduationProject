using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Queries.Models
{
    public record GetTemplateByIdQuery(Guid Id)
    : IRequest<Response<TemplateDetailDto>>;
}

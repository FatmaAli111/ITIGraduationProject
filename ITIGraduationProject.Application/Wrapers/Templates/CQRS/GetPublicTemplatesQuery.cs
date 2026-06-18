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
    public record GetPublicTemplatesQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Response<PaginatedResult<TemplateDto>>>;
}

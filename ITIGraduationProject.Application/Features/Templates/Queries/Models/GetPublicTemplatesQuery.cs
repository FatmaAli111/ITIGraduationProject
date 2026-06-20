using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Queries.Models
{
    public record GetPublicTemplatesQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Response<PaginatedResult<TemplateDto>>>;
}

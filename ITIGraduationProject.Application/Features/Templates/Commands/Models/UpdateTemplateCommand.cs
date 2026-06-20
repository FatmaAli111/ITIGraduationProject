using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Commands.Models
{
    public record UpdateTemplateCommand(
    Guid Id,
    string? Name,
    string? StyleTags,
    string? PreviewImageURL
) : IRequest<Response<TemplateDto>>;

}

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
    public record CreateTemplateCommand(
    string Name,
    Guid CategoryId,
    string? StyleTags,
    string PreviewImageURL,
    bool IsPublic = false
    ) : IRequest<Response<TemplateDto>>;
}

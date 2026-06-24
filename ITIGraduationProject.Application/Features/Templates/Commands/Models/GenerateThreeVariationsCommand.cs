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
    public record GenerateThreeVariationsCommand(Guid? ProductId = null, string UserMessage = "")
     : IRequest<Response<GenerateVariationsResponseDto>>;
}
using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Templates.CQRS
{
    public record DeleteTemplateCommand(Guid Id)
    : IRequest<Response<string>>;
}

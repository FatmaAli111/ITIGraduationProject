using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Shop.CQRS
{
    public record DeleteCategoryCommand(Guid Id)
    : IRequest<Response<string>>;
}

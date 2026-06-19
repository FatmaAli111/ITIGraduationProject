using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Wrapers.Shop.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Shop.CQRS
{
    public record CreateCategoryCommand(
    string Name,
    string? Description,
    string PrintableAreaConfig,
    string? ImageUrl
    ) : IRequest<Response<CategoryDto>>;
}

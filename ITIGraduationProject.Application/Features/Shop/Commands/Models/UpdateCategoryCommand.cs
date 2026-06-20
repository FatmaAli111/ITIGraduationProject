using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Models
{
    public record UpdateCategoryCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? PrintableAreaConfig,
    string? ImageUrl
    ) : IRequest<Response<CategoryDto>>;
}

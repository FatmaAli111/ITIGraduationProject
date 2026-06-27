using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using MediatR;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Models
{
    public record GetCurrentCartQuery : IRequest<Response<CartDto>>;
}

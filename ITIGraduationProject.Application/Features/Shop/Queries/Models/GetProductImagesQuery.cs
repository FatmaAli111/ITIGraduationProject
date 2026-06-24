using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using MediatR;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Models
{
    public record GetProductImagesQuery(Guid ProductId) : IRequest<Response<List<ProductImageDto>>>;
}

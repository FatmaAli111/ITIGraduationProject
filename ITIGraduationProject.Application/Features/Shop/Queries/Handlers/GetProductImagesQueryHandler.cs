using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Handlers
{
    public class GetProductImagesQueryHandler : ResponseHandler,
        IRequestHandler<GetProductImagesQuery, Response<List<ProductImageDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetProductImagesQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Response<List<ProductImageDto>>> Handle(GetProductImagesQuery request, CancellationToken ct)
        {
            var productImages = await _uow.ProductImages.GetTableNoTracking()
                .Where(x => x.ProductId == request.ProductId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.IsPrimary ? 0 : 1)
                .ToListAsync(ct);

            var dto = productImages.Adapt<List<ProductImageDto>>();
            return Success(dto);
        }
    }
}

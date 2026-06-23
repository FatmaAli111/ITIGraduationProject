using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.DTOS.ShopDTOs;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Handlers
{
    public class GetProductByIdQueryHandler : ResponseHandler,
        IRequestHandler<GetProductByIdQuery, Response<ProductDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetProductByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<Response<ProductDto>> Handle(
            GetProductByIdQuery request, CancellationToken ct)
        {
            var product = await _uow.Products.GetTableNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted);

            if (product == null)
                return NotFound<ProductDto>("Product not found");

            var dto = product.Adapt<ProductDto>();
            return Success(dto);
        }
    }
}

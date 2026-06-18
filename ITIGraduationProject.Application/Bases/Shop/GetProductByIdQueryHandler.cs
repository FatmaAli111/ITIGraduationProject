using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Application.Wrapers.Shop.CQRS;
using ITIGraduationProject.Application.Wrapers.Shop.DTOs;

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;

namespace ITIGraduationProject.Application.Bases.Shop
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
                .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted);

            if (product == null)
                return NotFound<ProductDto>("Product not found");

            return Success(product.Adapt<ProductDto>());
        }
    }
}

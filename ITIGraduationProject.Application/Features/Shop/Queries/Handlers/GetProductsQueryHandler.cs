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
    public class GetProductsQueryHandler: ResponseHandler,
        IRequestHandler<GetProductsQuery, Response<PaginatedResult<ProductDto>>>
    {
        private readonly IUnitOfWork _uow;
        public GetProductsQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<Response<PaginatedResult<ProductDto>>> Handle (
            GetProductsQuery request, CancellationToken ct)
        {
            var query = _uow.Products.GetTableNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted);

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            var result = await query
                        .ProjectToType<ProductDto>()
                        .ToPaginatedListAsync(request.PageNumber, request.PageSize);
            return Success(result);
        }
    }
}

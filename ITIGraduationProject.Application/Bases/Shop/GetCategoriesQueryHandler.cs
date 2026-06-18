using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Shop.CQRS;
using ITIGraduationProject.Application.Wrapers.Shop.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Shop
{
    public class GetCategoriesQueryHandler
    : ResponseHandler,
      IRequestHandler<GetCategoriesQuery, Response<List<CategoryDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetCategoriesQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<List<CategoryDto>>> Handle(
            GetCategoriesQuery request, CancellationToken ct)
        {
            var categories = await _uow.Products
                .GetTableNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .Select(p => p.Category)
                .Distinct()
                .ProjectToType<CategoryDto>()
                .ToListAsync(ct);

            return Success(categories);
        }
    }
}

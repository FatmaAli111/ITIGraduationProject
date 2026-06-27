using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Handlers
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
            var categories = await _uow.Categories
                .GetTableNoTracking()
                .Where(category => !category.IsDeleted)
                .OrderBy(category => category.Name)
                .ProjectToType<CategoryDto>()
                .ToListAsync(ct);

            return Success(categories);
        }
    }
}

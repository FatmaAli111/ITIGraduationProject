using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Products;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mapster;
namespace ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts
{
    public class GetStudioProductsQueryHandler : IRequestHandler<GetStudioProductsQuery, List<StudioProductListItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudioProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StudioProductListItemDto>> Handle(GetStudioProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _unitOfWork.Products
                .GetTableNoTracking()
                .ProjectToType<StudioProductListItemDto>()
                .Where(p => p.ThumbnailImageUrl != string.Empty)
                .ToListAsync(cancellationToken);

            return products;
        }
    }
}

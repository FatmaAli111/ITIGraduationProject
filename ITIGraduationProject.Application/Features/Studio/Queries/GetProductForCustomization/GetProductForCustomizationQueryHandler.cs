using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Products;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization
{
    public class GetProductForCustomizationQueryHandler : IRequestHandler<GetProductForCustomizationQuery, StudioProductDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetProductForCustomizationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StudioProductDetailDto> Handle(GetProductForCustomizationQuery request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products
                .GetTableNoTracking()
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                if (product == null)
                {
                    throw new KeyNotFoundException($"Entity \"{nameof(Product)}\" ({request.ProductId}) was not found.");
                }
            }

            return _mapper.Map<StudioProductDetailDto>(product);
        }
    }
}

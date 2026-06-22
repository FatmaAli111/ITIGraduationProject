using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetGraphicAssetById
{
    public class GetGraphicAssetByIdQueryHandler : IRequestHandler<GetGraphicAssetByIdQuery, GraphicAssetDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetGraphicAssetByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GraphicAssetDto?> Handle(GetGraphicAssetByIdQuery request, CancellationToken cancellationToken)
        {
            var asset = await _unitOfWork.GraphicAssets.GetTableNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null) return null;

            return asset.Adapt<GraphicAssetDto>();
        }
    }
}

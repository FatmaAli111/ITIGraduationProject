using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Designs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetGraphicAssets
{
    public class GetGraphicAssetsQueryHandler : IRequestHandler<GetGraphicAssetsQuery, List<GraphicAssetDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminUserService _identityService;

        public GetGraphicAssetsQueryHandler(IUnitOfWork unitOfWork, IAdminUserService adminUserService)
        {
            _unitOfWork = unitOfWork;
            _identityService = adminUserService;
        }

        public async Task<List<GraphicAssetDto>> Handle(GetGraphicAssetsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GraphicAssets.GetTableNoTracking();

            if (request.Type.HasValue)
            {
                query = query.Where(a => a.Type == request.Type.Value);
            }

            var assets = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
            var filteredAssets = new List<GraphicAsset>();

            foreach (var asset in assets)
            {
                var role = await _identityService.GetUserRoleAsync(asset.UserId);

                if (asset.UserId == request.CurrentUserId || role == "User")
                {
                    filteredAssets.Add(asset);
                }
            }

            return filteredAssets.Adapt<List<GraphicAssetDto>>();
        }
    }
}

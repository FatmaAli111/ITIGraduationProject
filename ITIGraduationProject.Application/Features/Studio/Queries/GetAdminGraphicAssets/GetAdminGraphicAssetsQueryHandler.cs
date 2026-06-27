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

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetAdminGraphicAssets
{
    public class GetAdminGraphicAssetsQueryHandler : IRequestHandler<GetAdminGraphicAssetsQuery, List<GraphicAssetDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminUserService _adminUserService;
        public GetAdminGraphicAssetsQueryHandler(IUnitOfWork unitOfWork, IAdminUserService adminUserService)
        {
            _unitOfWork = unitOfWork;
            _adminUserService = adminUserService;
        }

        public async Task<List<GraphicAssetDto>> Handle(GetAdminGraphicAssetsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GraphicAssets.GetTableNoTracking();

            if (request.Type.HasValue)
            {
                query = query.Where(a => a.Type == request.Type.Value);
            }

            var assets = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
            var adminAssets = new List<GraphicAsset>();

            foreach (var asset in assets)
            {  
                if (await _adminUserService.GetUserRoleAsync(asset.UserId) == "Admin")
                {
                    adminAssets.Add(asset);
                }
            }

            return adminAssets.Adapt<List<GraphicAssetDto>>();
        }
    }
}

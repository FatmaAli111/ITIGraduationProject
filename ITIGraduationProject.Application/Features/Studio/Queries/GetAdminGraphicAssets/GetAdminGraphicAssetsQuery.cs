using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetAdminGraphicAssets
{
    public record GetAdminGraphicAssetsQuery(GraphicAssetType? Type) : IRequest<List<GraphicAssetDto>>;
}

using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetGraphicAssetById
{
    public record GetGraphicAssetByIdQuery(Guid Id) : IRequest<GraphicAssetDto?>;
}

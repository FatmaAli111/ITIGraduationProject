using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset
{
    public record GraphicAssetDto(Guid Id, Guid UserId, string Name, GraphicAssetType Type, string ImageUrl, string? Tags);
}

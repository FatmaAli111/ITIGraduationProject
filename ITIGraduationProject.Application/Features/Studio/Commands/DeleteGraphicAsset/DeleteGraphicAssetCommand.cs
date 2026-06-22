using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Commands.DeleteGraphicAsset
{
    public record DeleteGraphicAssetCommand(Guid Id) : IRequest;
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization
{
    public record GetProductForCustomizationQuery(Guid ProductId) : IRequest<StudioProductDetailDto>;
}

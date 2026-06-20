using ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetUserDesigns
{
    public record GetUserDesignsQuery(Guid UserId) : IRequest<List<DesignResponseDto>>;
}

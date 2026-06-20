using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById
{
    public record GetDesignByIdQuery(Guid Id) : IRequest<DesignResponseDto>;
}

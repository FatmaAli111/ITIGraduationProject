using ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetUserDesigns
{
    public class GetUserDesignsQueryHandler : IRequestHandler<GetUserDesignsQuery, List<DesignResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserDesignsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DesignResponseDto>> Handle(GetUserDesignsQuery request, CancellationToken cancellationToken)
        {
            var userDesigns = await _unitOfWork.Designs
                .GetTableNoTracking()
                .Include(d => d.Product)
                .Where(d => d.UserId == request.UserId)
                .ProjectToType<DesignResponseDto>()
                .ToListAsync(cancellationToken);

            return userDesigns;
        }
    }
}

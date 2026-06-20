using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Designs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById
{
    public class GetDesignByIdQueryHandler : IRequestHandler<GetDesignByIdQuery, DesignResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDesignByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DesignResponseDto> Handle(GetDesignByIdQuery request, CancellationToken cancellationToken)
        {
            var designDto = await _unitOfWork.Designs
                .GetTableNoTracking()
                .Include(d => d.Product) 
                .Where(d => d.Id == request.Id)
                .ProjectToType<DesignResponseDto>()
                .FirstOrDefaultAsync(cancellationToken);

            if (designDto == null)
            {
                throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found.");
            }

            return designDto;
        }
    }
}

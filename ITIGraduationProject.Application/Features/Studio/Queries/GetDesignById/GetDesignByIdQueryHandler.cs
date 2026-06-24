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
            var design = await _unitOfWork.Designs
                .GetTableNoTracking()
                .Include(d => d.Product)
                .Include(d => d.GraphicAssets)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (design == null)
            {
                throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found.");
            }

            var designDto = new DesignResponseDto(
                design.Id,
                design.UserId,
                design.ProductId,
                design.Product?.Name ?? string.Empty,
                design.TemplateId,
                design.CanvasStateJSON,
                design.SnapshotImageURL,
                design.Status.ToString(),
                design.SelectedSize?.ToString(),
                design.SelectedFabric?.ToString(),
                design.SelectedPrintMethod?.ToString(),
                design.SelectedColor,
                design.CalculatedPrice
            );

            if (designDto == null)
            {
                throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found.");
            }

            return designDto;
        }
    }
}

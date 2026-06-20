using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;

namespace ITIGraduationProject.Application.CQRS.Handlers.Commands;

public class UpdateDesignCommandHandler : IRequestHandler<UpdateDesignCommand, Response<DesignDetailDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDesignRepository _designRepository;

    public UpdateDesignCommandHandler(
        IUnitOfWork unitOfWork,
        IDesignRepository designRepository)
    {
        _unitOfWork = unitOfWork;
        _designRepository = designRepository;
    }

    public async Task<Response<DesignDetailDTO>> Handle(UpdateDesignCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var design = await _designRepository.GetByIdAsync(request.DesignId);
            if (design == null)
                return new Response<DesignDetailDTO>
                {
                    Succeeded = false,
                    Message = "Design not found"
                };

            if (design.UserId != request.UserId)
                return new Response<DesignDetailDTO>
                {
                    Succeeded = false,
                    Message = "Unauthorized"
                };

            // Update design
            if (!string.IsNullOrEmpty(request.DesignName))
                design.CanvasStateJSON = request.DesignName; // map to existing property

            if (!string.IsNullOrEmpty(request.Colors))
                design.SnapshotImageURL = request.Colors; // map to existing property

            if (!string.IsNullOrEmpty(request.PrintLocation))
                design.SelectedColor = request.PrintLocation; // map to existing property

            design.UpdatedAt = DateTime.UtcNow;

            _designRepository.Update(design);
            await _unitOfWork.SaveChangesAsync();

            var designDto = new DesignDetailDTO
            {
                Id = design.Id,
                UserId = design.UserId,
                // map existing Design entity fields to DTO
                ProductId = design.ProductId,
                TemplateId = design.TemplateId,
                CanvasStateJSON = design.CanvasStateJSON,
                SnapshotImageURL = design.SnapshotImageURL,
                Status = design.Status,
                CreatedAt = design.CreatedAt,
                UpdatedAt = design.UpdatedAt
            };

            return new Response<DesignDetailDTO>
            {
                Succeeded = true,
                Data = designDto,
                Message = "Design updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new Response<DesignDetailDTO>
            {
                Succeeded = false,
                Message = $"Error updating design: {ex.Message}"
            };
        }
    }
}

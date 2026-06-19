using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.CQRS.Handlers.Commands;

public class CreateDesignCommandHandler : IRequestHandler<CreateDesignCommand, Response<DesignDetailDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDesignRepository _designRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITemplateRepository _templateRepository;

    public CreateDesignCommandHandler(
        IUnitOfWork unitOfWork,
        IDesignRepository designRepository,
        IProductRepository productRepository,
        ITemplateRepository templateRepository)
    {
        _unitOfWork = unitOfWork;
        _designRepository = designRepository;
        _productRepository = productRepository;
        _templateRepository = templateRepository;
    }

    public async Task<Response<DesignDetailDTO>> Handle(CreateDesignCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                return new Response<DesignDetailDTO>
                {
                    Succeeded = false,
                    Message = "Product not found"
                };

            // Validate template if provided
            if (request.TemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdAsync(request.TemplateId.Value);
                if (template == null)
                    return new Response<DesignDetailDTO>
                    {
                        Succeeded = false,
                        Message = "Template not found"
                    };
            }

            // Create design
            var design = new Design
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ProductId = request.ProductId,
                TemplateId = request.TemplateId,
                CanvasStateJSON = request.DesignName ?? string.Empty,
                SnapshotImageURL = request.Colors ?? string.Empty,
                SelectedColor = request.PrintLocation,
                Status = DesignStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _designRepository.AddAsync(design);
            await _unitOfWork.SaveChangesAsync();

            var designDto = new DesignDetailDTO
            {
                Id = design.Id,
                UserId = design.UserId,
                ProductId = design.ProductId,
                TemplateId = design.TemplateId,
                CanvasStateJSON = design.CanvasStateJSON,
                SnapshotImageURL = design.SnapshotImageURL,
                SelectedColor = design.SelectedColor,
                Status = design.Status,
                CreatedAt = design.CreatedAt,
                UpdatedAt = design.UpdatedAt
            };

            return new Response<DesignDetailDTO>
            {
                Succeeded = true,
                Data = designDto,
                Message = "Design created successfully"
            };
        }
        catch (Exception ex)
        {
            return new Response<DesignDetailDTO>
            {
                Succeeded = false,
                Message = $"Error creating design: {ex.Message}"
            };
        }
    }
}

using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Queries;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Repositories;

namespace ITIGraduationProject.Application.CQRS.Handlers.Queries;

public class GetDesignByIdQueryHandler : IRequestHandler<GetDesignByIdQuery, Response<DesignDetailDTO>>
{
    private readonly IDesignRepository _designRepository;

    public GetDesignByIdQueryHandler(IDesignRepository designRepository)
    {
        _designRepository = designRepository;
    }

    public async Task<Response<DesignDetailDTO>> Handle(GetDesignByIdQuery request, CancellationToken cancellationToken)
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

            var designDto = new DesignDetailDTO
            {
                Id = design.Id,
                UserId = design.UserId,
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
                Data = designDto
            };
        }
        catch (Exception ex)
        {
            return new Response<DesignDetailDTO>
            {
                Succeeded = false,
                Message = $"Error fetching design: {ex.Message}"
            };
        }
    }
}

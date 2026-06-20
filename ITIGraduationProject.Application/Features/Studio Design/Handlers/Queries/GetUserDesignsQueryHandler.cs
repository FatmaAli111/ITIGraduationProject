using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Queries;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Repositories;

namespace ITIGraduationProject.Application.CQRS.Handlers.Queries;

public class GetUserDesignsQueryHandler : IRequestHandler<GetUserDesignsQuery, Response<List<DesignListDTO>>>
{
    private readonly IDesignRepository _designRepository;

    public GetUserDesignsQueryHandler(IDesignRepository designRepository)
    {
        _designRepository = designRepository;
    }

    public async Task<Response<List<DesignListDTO>>> Handle(GetUserDesignsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var designs = (await _designRepository.GetByUserAsync(request.UserId)).ToList();
            if (designs == null || designs.Count == 0)
                return new Response<List<DesignListDTO>>
                {
                    Succeeded = true,
                    Data = new List<DesignListDTO>(),
                    Message = "No designs found"
                };

            var designDtos = designs.Select(d => new DesignListDTO
            {
                Id = d.Id,
                ProductName = d.Product?.Name ?? string.Empty,
                SnapshotImageURL = d.SnapshotImageURL,
                Status = d.Status,
                CalculatedPrice = d.CalculatedPrice,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt ?? d.CreatedAt
            }).ToList();

            return new Response<List<DesignListDTO>>
            {
                Succeeded = true,
                Data = designDtos
            };
        }
        catch (Exception ex)
        {
            return new Response<List<DesignListDTO>>
            {
                Succeeded = false,
                Message = $"Error fetching user designs: {ex.Message}"
            };
        }
    }
}

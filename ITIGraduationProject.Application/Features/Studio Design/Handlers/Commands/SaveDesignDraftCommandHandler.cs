using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.CQRS.Handlers.Commands;

public class SaveDesignDraftCommandHandler : IRequestHandler<SaveDesignDraftCommand, Response<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDesignRepository _designRepository;

    public SaveDesignDraftCommandHandler(
        IUnitOfWork unitOfWork,
        IDesignRepository designRepository)
    {
        _unitOfWork = unitOfWork;
        _designRepository = designRepository;
    }

    public async Task<Response<bool>> Handle(SaveDesignDraftCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var design = await _designRepository.GetByIdAsync(request.DesignId);
            if (design == null)
                return new Response<bool>
                {
                    Succeeded = false,
                    Message = "Design not found"
                };

            if (design.UserId != request.UserId)
                return new Response<bool>
                {
                    Succeeded = false,
                    Message = "Unauthorized"
                };

            design.Status = DesignStatus.Draft;
            design.UpdatedAt = DateTime.UtcNow;

            _designRepository.Update(design);
            await _unitOfWork.SaveChangesAsync();

            return new Response<bool>
            {
                Succeeded = true,
                Data = true,
                Message = "Design draft saved successfully"
            };
        }
        catch (Exception ex)
        {
            return new Response<bool>
            {
                Succeeded = false,
                Message = $"Error saving design draft: {ex.Message}"
            };
        }
    }
}

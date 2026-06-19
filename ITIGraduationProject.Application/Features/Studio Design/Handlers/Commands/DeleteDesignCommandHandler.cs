using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;

namespace ITIGraduationProject.Application.CQRS.Handlers.Commands;

public class DeleteDesignCommandHandler : IRequestHandler<DeleteDesignCommand, Response<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDesignRepository _designRepository;

    public DeleteDesignCommandHandler(
        IUnitOfWork unitOfWork,
        IDesignRepository designRepository)
    {
        _unitOfWork = unitOfWork;
        _designRepository = designRepository;
    }

    public async Task<Response<bool>> Handle(DeleteDesignCommand request, CancellationToken cancellationToken)
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

            _designRepository.Delete(design);
            await _unitOfWork.SaveChangesAsync();

            return new Response<bool>
            {
                Succeeded = true,
                Data = true,
                Message = "Design deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new Response<bool>
            {
                Succeeded = false,
                Message = $"Error deleting design: {ex.Message}"
            };
        }
    }
}

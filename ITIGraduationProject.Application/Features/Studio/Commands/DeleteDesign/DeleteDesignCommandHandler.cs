using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Studio.Commands.DeleteDesign;

public class DeleteDesignCommandHandler : IRequestHandler<DeleteDesignCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDesignCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDesignCommand request, CancellationToken cancellationToken)
    {
        var design = await _unitOfWork.Designs
            .GetTableAsTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (design == null)
        {
            throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found.");
        }

        _unitOfWork.Designs.Delete(design);

        await _unitOfWork.SaveChangesAsync();
    }
}

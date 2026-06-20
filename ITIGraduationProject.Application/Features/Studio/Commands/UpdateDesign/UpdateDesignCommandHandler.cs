using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;

public class UpdateDesignCommandHandler : IRequestHandler<UpdateDesignCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDesignCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDesignCommand request, CancellationToken cancellationToken)
    {
        var design = await _unitOfWork.Designs
            .GetTableAsTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (design == null)
        {
            throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found.");
        }

        request.Adapt(design);

        await _unitOfWork.SaveChangesAsync();
    }
}

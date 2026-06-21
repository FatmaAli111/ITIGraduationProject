using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public class CreateDesignCommandHandler : IRequestHandler<CreateDesignCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPriceCalculation _priceCalculationService;

    public CreateDesignCommandHandler(IUnitOfWork unitOfWork, IPriceCalculation priceCalculationService)
    {
        _unitOfWork = unitOfWork;
        _priceCalculationService = priceCalculationService;
    }

    public async Task<Guid> Handle(CreateDesignCommand request, CancellationToken cancellationToken)
    {
        var design = request.Adapt<Design>();

        design.Id = Guid.NewGuid();
        design.Status = DesignStatus.Draft;

     
        design.CalculatedPrice = await _priceCalculationService.CalculatePriceAsync(
            request.ProductId,
            request.SelectedFabric,
            request.SelectedPrintMethod,
            request.SelectedSize,
            cancellationToken
        );

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        return design.Id;
    }
}

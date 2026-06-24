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
using ITIGraduationProject.Domain.Entities.Identity;

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
        design.UserId = request.UserId;
        design.ProductId = request.ProductId;
        design.TemplateId = request.TemplateId;
        design.CanvasStateJSON = request.CanvasStateJSON;
        design.SnapshotImageURL = request.SnapshotImageURL;
        design.SelectedSize = request.SelectedSize;
        design.SelectedFabric = request.SelectedFabric;
        design.SelectedPrintMethod = request.SelectedPrintMethod;
        design.SelectedColor = request.SelectedColor;

        design.CalculatedPrice = await _priceCalculationService.CalculatePriceAsync(
            request.ProductId,
            request.SelectedFabric,
            request.SelectedPrintMethod,
            request.SelectedSize,
            cancellationToken
        );

        design.GraphicAssets = new List<GraphicAsset>();

        if (request.Assets is { Count: > 0 })
        {
            foreach (var assetInput in request.Assets)
            {
                var asset = new GraphicAsset
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Name = assetInput.Name,
                    Type = (GraphicAssetType)assetInput.Type,
                    ImageUrl = assetInput.ImageUrl,
                    Tags = assetInput.Tags,
                };

                design.GraphicAssets.Add(asset);
            }
        }

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        return design.Id;
    }
}

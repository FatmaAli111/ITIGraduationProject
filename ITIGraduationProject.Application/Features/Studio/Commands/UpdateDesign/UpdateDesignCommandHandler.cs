using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
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

        design.CanvasStateJSON = request.CanvasStateJSON;
        design.SnapshotImageURL = request.SnapshotImageURL;
        design.SelectedSize = request.SelectedSize;
        design.SelectedFabric = request.SelectedFabric;
        design.SelectedPrintMethod = request.SelectedPrintMethod;
        design.SelectedColor = request.SelectedColor;

        if (request.Assets is { Count: > 0 })
        {
            var existingAssets = design.GraphicAssets.ToList();
            foreach (var existingAsset in existingAssets)
            {
                design.GraphicAssets.Remove(existingAsset);
            }

            foreach (var assetInput in request.Assets)
            {
                design.GraphicAssets.Add(new GraphicAsset
                {
                    Id = Guid.NewGuid(),
                    UserId = design.UserId,
                    Name = assetInput.Name,
                    Type = (GraphicAssetType)assetInput.Type,
                    ImageUrl = assetInput.ImageUrl,
                    Tags = assetInput.Tags,
                });
            }
        }
        else
        {
            design.GraphicAssets.Clear();
        }

        await _unitOfWork.SaveChangesAsync();
    }
}

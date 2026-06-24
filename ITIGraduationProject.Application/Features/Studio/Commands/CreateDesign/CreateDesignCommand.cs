using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Domain.Enums;
using MediatR;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public record CreateDesignCommand(
    Guid UserId,
    Guid ProductId,
    Guid? TemplateId,
    string CanvasStateJSON,
    string SnapshotImageURL,
    ProductSize? SelectedSize,
    FabricType? SelectedFabric,
    PrintMethodType? SelectedPrintMethod,
    string? SelectedColor,
    List<DesignAssetInput>? Assets
) : IRequest<Guid>;

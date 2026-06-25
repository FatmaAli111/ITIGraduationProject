using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

namespace ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;

public record UpdateDesignCommand(
    Guid Id,
    string CanvasStateJSON,
    string SnapshotImageURL,
    ProductSize? SelectedSize,
    FabricType? SelectedFabric,
    PrintMethodType? SelectedPrintMethod,
    string? SelectedColor,
    List<DesignAssetInput>? Assets
) : IRequest;

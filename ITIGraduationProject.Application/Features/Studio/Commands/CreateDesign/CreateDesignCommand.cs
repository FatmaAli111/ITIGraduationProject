using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Domain.Enums;
using MediatR;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public record CreateDesignCommand(
    Guid? Id,
    Guid ProductId,
    Guid? TemplateId,
    string CanvasStateJSON,
    string? Base64Snapshot,
    string? Base64Front,
    string? Base64Back,
    ProductSize? SelectedSize,
    FabricType? SelectedFabric,
    PrintMethodType? SelectedPrintMethod,
    string? SelectedColor,
    List<DesignAssetInput>? Assets
) : IRequest<Guid>;

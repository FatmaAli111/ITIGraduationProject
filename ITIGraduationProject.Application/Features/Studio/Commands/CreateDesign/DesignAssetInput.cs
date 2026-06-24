namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public record DesignAssetInput(
    string Name,
    int Type,
    string ImageUrl,
    string? Tags
);

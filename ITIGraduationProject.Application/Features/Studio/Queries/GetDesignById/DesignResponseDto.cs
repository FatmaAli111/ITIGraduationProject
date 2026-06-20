using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById
{
    public record DesignResponseDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    string ProductName,
    Guid? TemplateId,
    string CanvasStateJSON,
    string SnapshotImageURL,
    string Status,
    string? SelectedSize,
    string? SelectedFabric,
    string? SelectedPrintMethod,
    string? SelectedColor,
    decimal CalculatedPrice
);
}

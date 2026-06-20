using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;

namespace ITIGraduationProject.Application.CQRS.Commands;

public class CalculatePriceCommand : IRequest<Response<PriceCalculationResultDTO>>
{
    public DesignPriceCalculationDTO PriceCalculation { get; set; } = null!;
}

using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.CQRS.Handlers.Commands;

public class CalculatePriceCommandHandler : IRequestHandler<CalculatePriceCommand, Response<PriceCalculationResultDTO>>
{
    private readonly IProductRepository _productRepository;

    public CalculatePriceCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Response<PriceCalculationResultDTO>> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.PriceCalculation.ProductId);
            if (product == null)
                return new Response<PriceCalculationResultDTO>
                {
                    Succeeded = false,
                    Message = "Product not found"
                };

            var basePrice = product.BasePrice;

            // Calculate surcharges
            var fabricSurcharge = CalculateFabricSurcharge(request.PriceCalculation.SelectedFabric);
            var printMethodSurcharge = CalculatePrintMethodSurcharge(request.PriceCalculation.SelectedPrintMethod);
            var sizeSurcharge = CalculateSizeSurcharge(request.PriceCalculation.SelectedSize);

            var totalPrice = basePrice + fabricSurcharge + printMethodSurcharge + sizeSurcharge;

            var result = new PriceCalculationResultDTO
            {
                BasePrice = basePrice,
                FabricSurcharge = fabricSurcharge,
                PrintMethodSurcharge = printMethodSurcharge,
                SizeSurcharge = sizeSurcharge,
                TotalPrice = totalPrice
            };

            return new Response<PriceCalculationResultDTO>
            {
                Succeeded = true,
                Data = result,
                Message = "Price calculated successfully"
            };
        }
        catch (Exception ex)
        {
            return new Response<PriceCalculationResultDTO>
            {
                Succeeded = false,
                Message = $"Error calculating price: {ex.Message}"
            };
        }
    }

    private decimal CalculateFabricSurcharge(FabricType? fabricType)
    {
        return fabricType switch
        {
            FabricType.Cotton => 5.00m,
            FabricType.Polyester => 3.50m,
            FabricType.Silk => 10.00m,
            _ => 0m
        };
    }

    private decimal CalculatePrintMethodSurcharge(PrintMethodType? printMethodType)
    {
        return printMethodType switch
        {
            PrintMethodType.ScreenPrinting => 8.00m,
            PrintMethodType.DirectToGarment => 12.00m,
            PrintMethodType.Embroidery => 15.00m,
            _ => 0m
        };
    }

    private decimal CalculateSizeSurcharge(ProductSize? size)
    {
        return size switch
        {
            ProductSize.L => 2.00m,
            ProductSize.XL => 4.00m,
            ProductSize.XXL => 6.00m,
            _ => 0m
        };
    }
}

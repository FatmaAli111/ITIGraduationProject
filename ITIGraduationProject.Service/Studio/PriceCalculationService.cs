using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;

namespace ITIGraduationProject.Service.Studio
{
    public class PriceCalculationService : IPriceCalculation
    {
        private readonly IUnitOfWork _unitOfWork;

        public PriceCalculationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CalculatePriceAsync(
            Guid productId,
            FabricType? selectedFabric,
            PrintMethodType? selectedPrintMethod,
            ProductSize? selectedSize,
            CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products
                .GetTableNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
            {
                throw new KeyNotFoundException($"Entity \"Product\" ({productId}) was not found.");
            }

            decimal total = product.BasePrice;

            if (selectedFabric.HasValue)
            {
                total += selectedFabric.Value switch
                {
                    FabricType.Cotton => 50.00m,
                    FabricType.Polyester => 30.00m,
                    FabricType.Wool => 70.00m,
                    FabricType.Silk => 100.00m,
                    FabricType.Linen => 80.00m,
                    _ => 0.00m
                };
            }

            if (selectedPrintMethod.HasValue)
            {
                total += selectedPrintMethod.Value switch
                {
                    PrintMethodType.ScreenPrinting => 20.00m,
                    PrintMethodType.HeatTransfer => 15.00m,
                    PrintMethodType.Sublimation => 25.00m,
                    PrintMethodType.Embroidery => 30.00m,
                    PrintMethodType.DirectToGarment => 10.00m,
                    _ => 0.00m
                };
            }

            if (selectedSize.HasValue)
            {
                total += selectedSize.Value switch
                {
                    ProductSize.XXL => 25.00m,
                    ProductSize.XXXL => 35.00m,
                    ProductSize.XS => 10.00m,
                    ProductSize.S => 15.00m,
                    ProductSize.M => 20.00m,
                    ProductSize.L => 25.00m,
                    ProductSize.XL => 30.00m,
                    _ => 0.00m
                };
            }

            return total;
        }
    }
}

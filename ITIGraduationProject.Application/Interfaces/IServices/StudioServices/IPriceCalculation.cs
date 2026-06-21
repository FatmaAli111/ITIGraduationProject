using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.StudioServices
{
    public interface IPriceCalculation
    {
        Task<decimal> CalculatePriceAsync(
                Guid productId,
                FabricType? selectedFabric,
                PrintMethodType? selectedPrintMethod,
                ProductSize? selectedSize,
                CancellationToken cancellationToken = default);
    }
}

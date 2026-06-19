using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Wrapers.Shop.DTOs;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Shop.CQRS
{
    public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    decimal BasePrice,
    ProductAvailableColors AvailableColors,
    string PreviewImageURL,
    bool IsAvailable,
    string StockStatus
) : IRequest<Response<ProductDto>>;
}

using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Models
{
    public record UpdateProductCommand(
    Guid Id,
    string? Name,
    decimal? BasePrice,
    ProductAvailableColors? AvailableColors,
    string? PreviewImageURL,
    bool? IsAvailable,
    string? StockStatus
    ) : IRequest<Response<ProductDto>>;
}

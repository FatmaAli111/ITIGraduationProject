using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Application.DTOS.ShopDTOs;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Models
{
    public record GetProductsQuery(
     int PageNumber = 1,
     int PageSize = 10,
     Guid? CategoryId = null
    ) : IRequest<Response<PaginatedResult<ProductDto>>>;
}

using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using ITIGraduationProject.Application.Features.Admin.Orders.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Admin.Orders.Queries.Handlers
{
    public class GetAllOrdersQueryHandler
        : ResponseHandler,
          IRequestHandler<GetAllOrdersQuery, Response<PaginatedResult<RecentOrderDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetAllOrdersQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<PaginatedResult<RecentOrderDto>>> Handle(
            GetAllOrdersQuery request, CancellationToken ct)
        {
            var query = _uow.Orders
                .GetTableNoTracking()
                .Where(o => !o.IsDeleted);

            if (request.Status.HasValue)
                query = query.Where(o => o.OrderStatus == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(o =>
                    o.OrderNumber.Contains(term) ||
                    o.User.Name.Contains(term));
            }

            var result = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.Name,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Success(result);
        }
    }
}

using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Handlers
{
    public class GetRecentOrdersQueryHandler
        : ResponseHandler,
          IRequestHandler<GetRecentOrdersQuery, Response<List<RecentOrderDto>>>
    {
        private const int MaxCount = 50;
        private readonly IUnitOfWork _uow;

        public GetRecentOrdersQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<List<RecentOrderDto>>> Handle(
            GetRecentOrdersQuery request, CancellationToken ct)
        {
            var count = request.Count <= 0 ? 10 : request.Count;
            if (count > MaxCount)
                count = MaxCount;

            var orders = await _uow.Orders
                .GetTableNoTracking()
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.Name,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(ct);

            return Success(orders);
        }
    }
}

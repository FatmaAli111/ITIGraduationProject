using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Handlers
{
    public class GetOrdersByStatusQueryHandler
        : ResponseHandler,
          IRequestHandler<GetOrdersByStatusQuery, Response<List<OrderStatusCountDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetOrdersByStatusQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<List<OrderStatusCountDto>>> Handle(
            GetOrdersByStatusQuery request, CancellationToken ct)
        {
            var countsByStatus = await _uow.Orders
                .GetTableNoTracking()
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var countLookup = countsByStatus.ToDictionary(x => x.Status, x => x.Count);

            var result = Enum.GetValues<OrderStatus>()
                .Select(status => new OrderStatusCountDto
                {
                    Status = status.ToString(),
                    Count = countLookup.GetValueOrDefault(status, 0)
                })
                .ToList();

            return Success(result);
        }
    }
}

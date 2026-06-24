using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Handlers
{
    public class GetDashboardOverviewQueryHandler
        : ResponseHandler,
          IRequestHandler<GetDashboardOverviewQuery, Response<DashboardOverviewDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetDashboardOverviewQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<DashboardOverviewDto>> Handle(
            GetDashboardOverviewQuery request, CancellationToken ct)
        {
            var users = _uow.Users.GetTableNoTracking().Where(u => !u.IsDeleted);
            var orders = _uow.Orders.GetTableNoTracking().Where(o => !o.IsDeleted);
            var products = _uow.Products.GetTableNoTracking().Where(p => !p.IsDeleted);
            var templates = _uow.Templates.GetTableNoTracking().Where(t => !t.IsDeleted);
            var reports = _uow.ModerationReports.GetTableNoTracking().Where(r => !r.IsDeleted);

            var overview = new DashboardOverviewDto
            {
                TotalUsers = await users.CountAsync(ct),
                TotalOrders = await orders.CountAsync(ct),
                TotalRevenue = await orders
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled
                             && o.OrderStatus != OrderStatus.Returned)
                    .SumAsync(o => o.TotalAmount, ct),
                PendingOrders = await orders
                    .CountAsync(o => o.OrderStatus == OrderStatus.Pending, ct),
                TotalProducts = await products.CountAsync(ct),
                TotalTemplates = await templates.CountAsync(ct),
                PublicTemplates = await templates.CountAsync(t => t.IsPublic, ct),
                PendingModerationReports = await reports
                    .CountAsync(r => r.Status == ModerationReportStatus.Pending, ct)
            };

            return Success(overview);
        }
    }
}

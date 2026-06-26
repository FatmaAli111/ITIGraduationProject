using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using MediatR;

namespace ITIGraduationProject.Application.Features.Admin.Orders.Queries.Models
{
    public record GetAllOrdersQuery(
        int PageNumber = 1,
        int PageSize = 20,
        OrderStatus? Status = null,
        string? Search = null)
        : IRequest<Response<PaginatedResult<RecentOrderDto>>>;
}

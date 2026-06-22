using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using MediatR;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models
{
    public record GetOrdersByStatusQuery() : IRequest<Response<List<OrderStatusCountDto>>>;
}

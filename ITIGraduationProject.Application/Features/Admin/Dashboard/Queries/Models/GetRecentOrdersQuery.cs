using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using MediatR;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models
{
    public record GetRecentOrdersQuery(int Count = 10) : IRequest<Response<List<RecentOrderDto>>>;
}

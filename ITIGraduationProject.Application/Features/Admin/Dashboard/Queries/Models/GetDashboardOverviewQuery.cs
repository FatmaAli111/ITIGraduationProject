using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.Dashboard;
using MediatR;

namespace ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models
{
    public record GetDashboardOverviewQuery() : IRequest<Response<DashboardOverviewDto>>;
}

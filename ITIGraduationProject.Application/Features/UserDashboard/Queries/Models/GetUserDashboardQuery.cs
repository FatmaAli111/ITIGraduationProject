using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.UserDashboard;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.UserDashboard.Queries.Models
{
    public record GetUserDashboardQuery(Guid UserId) : IRequest<Response<UserDashboardDto>>;
}

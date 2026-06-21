using ITIGraduationProject.Application.Bases;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record ResolveModerationReportCommand(Guid Id, string ActionTaken) : IRequest<Response<string>>;
}

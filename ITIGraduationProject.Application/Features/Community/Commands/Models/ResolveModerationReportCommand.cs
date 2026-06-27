using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record ResolveModerationReportCommand(
        Guid Id,
        string? ActionTaken,
        ModerationReportStatus? Status = null) : IRequest<Response<string>>;
}

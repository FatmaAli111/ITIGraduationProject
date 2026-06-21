using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class ResolveModerationReportCommandHandler
        : ResponseHandler,
          IRequestHandler<ResolveModerationReportCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;

        public ResolveModerationReportCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<string>> Handle(
            ResolveModerationReportCommand cmd, CancellationToken ct)
        {
            var report = await _uow.ModerationReports.GetByIdAsync(cmd.Id);
            if (report is null || report.IsDeleted)
                return NotFound<string>("Report not found");

            report.ActionTaken = cmd.ActionTaken;
            report.Status = ModerationReportStatus.ActionTaken;
            report.ResolvedAt = DateTime.UtcNow;

            _uow.ModerationReports.Update(report);
            await _uow.SaveChangesAsync();

            return Success("Report resolved successfully");
        }
    }
}

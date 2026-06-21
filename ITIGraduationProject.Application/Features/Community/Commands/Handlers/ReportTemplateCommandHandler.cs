using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class ReportTemplateCommandHandler
        : ResponseHandler,
          IRequestHandler<ReportTemplateCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public ReportTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<string>> Handle(
            ReportTemplateCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.TemplateId);
            if (template is null || template.IsDeleted)
                return NotFound<string>("Template not found");

            var report = new ModerationReport
            {
                ReporterUserId = _currentUser.UserId,
                TargetTemplateId = cmd.TemplateId,
                Reason = cmd.Reason,
                Status = ModerationReportStatus.Pending
            };

            await _uow.ModerationReports.AddAsync(report);
            await _uow.SaveChangesAsync();

            return Created("Report submitted successfully");
        }
    }
}

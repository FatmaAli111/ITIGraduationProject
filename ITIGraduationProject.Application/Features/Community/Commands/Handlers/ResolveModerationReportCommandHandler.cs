using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Features.Notifications;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly INotificationService _notificationService;
        private readonly ILogger<ResolveModerationReportCommandHandler> _logger;

        public ResolveModerationReportCommandHandler(
            IUnitOfWork uow,
            INotificationService notificationService,
            ILogger<ResolveModerationReportCommandHandler> logger)
            => (_uow, _notificationService, _logger) = (uow, notificationService, logger);

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

            var template = await _uow.Templates.GetByIdAsync(report.TargetTemplateId);
            if (template is not null && !template.IsDeleted)
            {
                await NotificationDispatchHelper.TrySendAsync(
                    _notificationService,
                    _logger,
                    template.CreatorUserId,
                    "Moderation update",
                    $"Action taken on your template \"{template.Name}\": {cmd.ActionTaken}.",
                    NotificationType.ModerationUpdate);
            }

            return Success("Report resolved successfully");
        }
    }
}

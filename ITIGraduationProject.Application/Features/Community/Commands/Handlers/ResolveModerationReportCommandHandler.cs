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

            var nextStatus = cmd.Status ?? ModerationReportStatus.ActionTaken;
            var actionTaken = cmd.ActionTaken?.Trim();

            if (nextStatus == ModerationReportStatus.ActionTaken && string.IsNullOrWhiteSpace(actionTaken))
                return BadRequest<string>("Action taken is required when resolving a report.");

            if (!string.IsNullOrWhiteSpace(actionTaken))
                report.ActionTaken = actionTaken;

            report.Status = nextStatus;
            report.ResolvedAt = nextStatus is ModerationReportStatus.ActionTaken or ModerationReportStatus.Dismissed
                ? DateTime.UtcNow
                : null;

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
                    BuildNotificationMessage(template.Name, nextStatus, actionTaken),
                    NotificationType.ModerationUpdate);
            }

            return Success("Report status updated successfully");
        }

        private static string BuildNotificationMessage(
            string templateName,
            ModerationReportStatus status,
            string? actionTaken)
        {
            if (!string.IsNullOrWhiteSpace(actionTaken))
                return $"Moderation status for your template \"{templateName}\" changed to {status}: {actionTaken}.";

            return $"Moderation status for your template \"{templateName}\" changed to {status}.";
        }
    }
}

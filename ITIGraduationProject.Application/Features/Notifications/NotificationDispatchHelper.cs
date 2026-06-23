using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Notifications
{
    internal static class NotificationDispatchHelper
    {
        internal static async Task TrySendAsync(
            INotificationService notificationService,
            ILogger logger,
            Guid userId,
            string title,
            string message,
            NotificationType type)
        {
            try
            {
                var result = await notificationService.SendNotificationAsync(
                    userId, title, message, type);

                if (!result.Succeeded)
                {
                    logger.LogWarning(
                        "Notification dispatch returned unsuccessful for user {UserId}. Type: {Type}. Message: {Message}",
                        userId,
                        type,
                        result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to send notification to user {UserId}. Type: {Type}",
                    userId,
                    type);
            }
        }
    }
}

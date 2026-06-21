using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community
{
    internal static class CommunityNotificationHelper
    {
        internal static async Task TrySendAsync(
            INotificationService notificationService,
            Guid userId,
            string title,
            string message,
            NotificationType type)
        {
            try
            {
                await notificationService.SendNotificationAsync(userId, title, message, type);
            }
            catch
            {
                // Notification delivery must not block the community action.
            }
        }
    }
}

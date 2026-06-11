using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Application.Repositories
{
    public interface INotificationRepository : IGenericRepo<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadByUserAsync(Guid userId);
        Task<IEnumerable<Notification>> GetByUserAsync(Guid userId);
    }
}

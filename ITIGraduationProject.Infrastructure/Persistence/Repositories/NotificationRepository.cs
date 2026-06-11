using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : GenericRepo<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }
    }
}

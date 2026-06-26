using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class AiChatSessionRepository : GenericRepo<AiChatSession>, IAiChatSessionRepository
    {
        public AiChatSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<AiChatSession?> GetWithMessagesAsync(Guid id)
        {
            return await _context.AiChatSessions
                .Include(s => s.AiChatMessages)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<AiChatSession>> GetByUserAsync(Guid userId)
        {
            return await _context.AiChatSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }
        public async Task<AiChatSession?> GetWithMessagesTrackingAsync(Guid id)
        {
            return await _context.AiChatSessions
                .Include(s => s.AiChatMessages)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}

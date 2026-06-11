using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IAiChatSessionRepository : IGenericRepo<AiChatSession>
    {
        Task<AiChatSession?> GetWithMessagesAsync(Guid id);
        Task<IEnumerable<AiChatSession>> GetByUserAsync(Guid userId);
    }
}

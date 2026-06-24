using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class AiChatMessageRepository
    : GenericRepo<AiChatMessage>,
      IAiChatMessageRepository
    {
        public AiChatMessageRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}

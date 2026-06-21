using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class CommunityInteractionRepository : GenericRepo<CommunityInteraction>, ICommunityInteractionRepository
    {
        public CommunityInteractionRepository(AppDbContext context) : base(context)
        {
        }
    }
}

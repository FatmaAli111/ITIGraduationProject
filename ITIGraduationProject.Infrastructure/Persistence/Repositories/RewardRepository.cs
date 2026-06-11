using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class RewardRepository : GenericRepo<Reward>, IRewardRepository
    {
        public RewardRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reward>> GetUnclaimedByUserAsync(Guid userId)
        {
            return await _context.Rewards
                .AsNoTracking()
                .Where(r => r.UserId == userId && !r.IsClaimed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reward>> GetByUserAsync(Guid userId)
        {
            return await _context.Rewards
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}

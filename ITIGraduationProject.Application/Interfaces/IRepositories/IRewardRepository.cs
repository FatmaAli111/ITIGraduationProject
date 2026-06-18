using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IRewardRepository : IGenericRepo<Reward>
    {
        Task<IEnumerable<Reward>> GetUnclaimedByUserAsync(Guid userId);
        Task<IEnumerable<Reward>> GetByUserAsync(Guid userId);
    }
}

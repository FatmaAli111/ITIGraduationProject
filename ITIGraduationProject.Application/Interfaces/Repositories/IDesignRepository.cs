using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IDesignRepository : IGenericRepo<Design>
    {
        Task<Design?> GetWithImagesAndAssetsAsync(Guid id);
        Task<IEnumerable<Design>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Design>> GetByStatusAsync(DesignStatus status);
    }
}

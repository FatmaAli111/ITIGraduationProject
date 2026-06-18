using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Products;

namespace ITIGraduationProject.Application.Repositories
{
    public interface ITemplateRepository : IGenericRepo<Template>
    {
        Task<Template?> GetWithDetailsAsync(Guid id);
        Task<IEnumerable<Template>> GetPublicTemplatesAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<Template>> GetByCreatorAsync(Guid creatorUserId);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IUserRepository : IGenericRepo<User>
    {
        Task<User?> GetWithProfileCartAndPreferencesAsync(Guid id);
        Task<User?> GetByNameAsync(string name);
    }
}

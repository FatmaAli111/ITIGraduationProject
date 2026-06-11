using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepo<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetWithProfileCartAndPreferencesAsync(Guid id)
        {
            return await _context.Set<User>()
                .AsNoTracking()
                .Include(u => u.UserPreferences)
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByNameAsync(string name)
        {
            return await _context.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == name);
        }
    }
}

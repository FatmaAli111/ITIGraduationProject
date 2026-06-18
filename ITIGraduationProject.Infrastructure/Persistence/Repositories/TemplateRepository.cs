using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class TemplateRepository : GenericRepo<Template>, ITemplateRepository
    {
        public TemplateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Template?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Templates
                .AsNoTracking()
                .Include(t => t.Designs)
                .Include(t => t.Rewards)
                .Include(t => t.ModerationReports)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetPublicTemplatesAsync(int page = 1, int pageSize = 20)
        {
            return await _context.Templates
                .AsNoTracking()
                .Where(t => t.IsPublic)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetByCreatorAsync(Guid creatorUserId)
        {
            return await _context.Templates
                .AsNoTracking()
                .Where(t => t.CreatorUserId == creatorUserId)
                .ToListAsync();
        }
    }
}

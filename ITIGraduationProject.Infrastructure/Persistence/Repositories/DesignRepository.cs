using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class DesignRepository : GenericRepo<Design>, IDesignRepository
    {
        public DesignRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Design?> GetWithImagesAndAssetsAsync(Guid id)
        {
            return await _context.Designs
                .AsNoTracking()
                .Include(d => d.DesignImages)
                .Include(d => d.GraphicAssets)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Design>> GetByUserAsync(Guid userId)
        {
            return await _context.Designs
                .AsNoTracking()
                .Include(d => d.Product)
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Design>> GetByStatusAsync(DesignStatus status)
        {
            return await _context.Designs
                .AsNoTracking()
                .Where(d => d.Status == status)
                .ToListAsync();
        }
    }
}

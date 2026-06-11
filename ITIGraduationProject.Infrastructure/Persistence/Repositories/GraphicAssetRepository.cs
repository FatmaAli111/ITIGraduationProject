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
    public class GraphicAssetRepository : GenericRepo<GraphicAsset>, IGraphicAssetRepository
    {
        public GraphicAssetRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GraphicAsset>> SearchByTagsAsync(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return Enumerable.Empty<GraphicAsset>();

            return await _context.GraphicAssets
                .AsNoTracking()
                .Where(a => a.Tags != null && a.Tags.Contains(tags))
                .ToListAsync();
        }

        public async Task<IEnumerable<GraphicAsset>> GetByTypeAsync(GraphicAssetType type)
        {
            return await _context.GraphicAssets
                .AsNoTracking()
                .Where(a => a.Type == type)
                .ToListAsync();
        }
    }
}

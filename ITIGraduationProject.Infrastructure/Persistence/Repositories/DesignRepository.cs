using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        /// <inheritdoc />
        public async Task SetGraphicAssetsAsync(
            Design design,
            IList<GraphicAsset> assets,
            CancellationToken cancellationToken = default)
        {
            var designEntry = _context.Entry(design);

           
            if (designEntry.State != EntityState.Added)
            {
                if (!designEntry.Collection(d => d.GraphicAssets).IsLoaded)
                {
                    await designEntry.Collection(d => d.GraphicAssets).LoadAsync(cancellationToken);
                }
            }
            else
            {
                design.GraphicAssets.Clear();
            }

            var desiredIds = assets.Select(a => a.Id).ToHashSet();
            var currentIds = design.GraphicAssets.Select(a => a.Id).ToHashSet();

            var toRemove = design.GraphicAssets
                .Where(a => !desiredIds.Contains(a.Id))
                .ToList();

            foreach (var asset in toRemove)
            {
                design.GraphicAssets.Remove(asset);
            }

            foreach (var asset in assets)
            {
                if (currentIds.Contains(asset.Id))
                    continue;

                
                var assetEntry = _context.Entry(asset);
                if (assetEntry.State == EntityState.Detached)
                {
                    _context.Attach(asset);
                }

                design.GraphicAssets.Add(asset);
            }
        }

        /// <inheritdoc />
        public List<string> GetChangeTrackerState()
        {
            return _context.ChangeTracker.Entries()
                .Select(e => $"Type: {e.Entity.GetType().Name}, State: {e.State}")
                .ToList();
        }

        /// <inheritdoc />
        public async Task<int> GetDesignGraphicAssetsCountAsync(Guid designId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM DesignGraphicAssets WHERE DesignsId = @designId";
                
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@designId";
                parameter.Value = designId;
                command.Parameters.Add(parameter);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync(cancellationToken);
                }

                var result = await command.ExecuteScalarAsync(cancellationToken);
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                // In case database isn't fully set up or other errors, log it as -1
                return -1;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
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

        /// <summary>
        /// Replaces the DesignGraphicAssets join-table rows for the given design
        /// so that they exactly match <paramref name="assets"/>.
        /// Existing rows not in the new list are deleted; new rows are inserted.
        /// The GraphicAsset records themselves are never deleted.
        /// </summary>
        Task SetGraphicAssetsAsync(Design design, IList<GraphicAsset> assets, CancellationToken cancellationToken = default);

        /// <summary>
        /// Diagnostic method to inspect the ChangeTracker state immediately before saving.
        /// </summary>
        List<string> GetChangeTrackerState();

        /// <summary>
        /// Diagnostic method to directly query the SQL database join table DesignGraphicAssets.
        /// </summary>
        Task<int> GetDesignGraphicAssetsCountAsync(Guid designId, CancellationToken cancellationToken = default);
    }
}

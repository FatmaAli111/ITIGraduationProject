using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IGraphicAssetRepository : IGenericRepo<GraphicAsset>
    {
        Task<IEnumerable<GraphicAsset>> SearchByTagsAsync(string? tags);
        Task<IEnumerable<GraphicAsset>> GetByTypeAsync(GraphicAssetType type);
    }
}

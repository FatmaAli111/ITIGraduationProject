using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.Products;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IProductRepository : IGenericRepo<Product>
    {
        Task<Product?> GetWithImagesAndDesignsAsync(Guid id);
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
    }
}

using System;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Application.Repositories
{
    public interface ICartRepository : IGenericRepo<Cart>
    {
        Task<Cart?> GetCurrentCartAsync(Guid userId);
        Task<Cart> CreateCartAsync(Guid userId);
        Task<Cart?> GetCartWithItemsAsync(Guid userId);
    }
}

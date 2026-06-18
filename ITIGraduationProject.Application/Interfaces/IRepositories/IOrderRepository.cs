using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IOrderRepository : IGenericRepo<Order>
    {
        Task<Order?> GetWithItemsAndShipmentAsync(Guid id);
        Task<IEnumerable<Order>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    }
}

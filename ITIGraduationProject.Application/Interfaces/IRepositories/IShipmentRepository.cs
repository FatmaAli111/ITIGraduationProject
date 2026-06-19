using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IShipmentRepository : IGenericRepo<Shipment>
    {
        Task<Shipment?> GetWithLogsAsync(Guid id);
        Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);
    }
}

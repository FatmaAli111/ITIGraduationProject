using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class ShipmentRepository : GenericRepo<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Shipment?> GetWithLogsAsync(Guid id)
        {
            return await _context.Shipments
                .AsNoTracking()
                .Include(s => s.ShipmentLogs)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _context.Shipments
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }
    }
}

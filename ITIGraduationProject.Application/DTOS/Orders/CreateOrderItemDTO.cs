using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.Orders
{
    public class CreateOrderItemDTO
    {
        public Guid DesignId { get; set; }
        public int Quantity { get; set; }
        public Guid? PrinterProfileId { get; set; }
    }
}

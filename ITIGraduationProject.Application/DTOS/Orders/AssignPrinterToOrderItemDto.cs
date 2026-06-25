using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.Orders
{
    public class AssignPrinterToOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public OrderItemStatus Status { get; set; }
        public Guid PrinterProfileId { get; set; }
    }
}

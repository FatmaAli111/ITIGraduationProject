using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.PrinterDashboard
{
    public class PrinterOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string DesignSnapshotUrl { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }
}

using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.PrinterDashboard
{
    public class PrinterProfileSummaryDto
    {
        public Guid ProfileId { get; set; }
        public FabricType SupportedFabrics { get; set; }
        public PrintMethodType SupportedPrintMethods { get; set; }
        public bool IsActive { get; set; }
        public int TotalAssignedItems { get; set; }
        public int PendingItems { get; set; }
        public int CompletedItems { get; set; }
    }
}

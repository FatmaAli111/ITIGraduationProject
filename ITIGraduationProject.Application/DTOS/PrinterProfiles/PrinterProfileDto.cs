using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.PrinterProfiles
{
    public class PrinterProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public FabricType SupportedFabrics { get; set; }
        public PrintMethodType SupportedPrintMethods { get; set; }
        public bool IsActive { get; set; }
        public string? PrinterName { get; set; }
    }
}

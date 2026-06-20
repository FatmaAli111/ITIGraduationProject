using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization
{
    public class StudioMockupDetailDto
    {
        public Guid Id { get; set; }
        public object? Color { get; set; }
        public string ViewAngle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public PrintableZoneDetailDto? PrintableZone { get; set; }
    }
}
